using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.Core;
using VRC.SDK3.Components;
using VRC.SDK3.Components.Video;
using VRC.SDK3.Video.Components.Base;
using VRC.SDKBase;
using VRC.Udon.Wrapper.Modules;

namespace Kinel.VideoPlayer.Scripts
{
    public class KinelVideoScript : UdonSharpBehaviour
    {
        public BaseVRCVideoPlayer videoPlayer;
        public VideoTimeSliderController sliderController;
        public InitializeScript initializeSystemObject; 
        public VRCUrlInputField url;
        public VideoLoadErrorController videoLoadErrorController;
        public Text debugTextComponant;
        public GameObject inputFieldLoadMessage;
        public bool isStream = false;
        
        private const int SyncWaitSeconds = 3;
        private bool globalSync = false, localSync = false, joinSync = false, sliderTimeSync = false;
        private float lastSyncTime, syncFrequency = 0.5f, deleyLimit = 0.5f;
        private int globalSyncUpdateWaitCount = 0, joinSyncUpdateWaitCount = 0, sliderTimeSyncUpdateWaitCount = 0;
        private int syncCount = 0, joinSyncCount = 0;
        private int[] videoTime = new int[3];
        private string debugText;
        
        [UdonSynced] private VRCUrl syncedURL;
        [UdonSynced] public bool isPlaying = false;
        [UdonSynced] public bool isPause = false;
        [UdonSynced] private float videoStartGlobalTime = 0;
        [UdonSynced] private float pausedTime = 0;
        [UdonSynced] private float pauseElapsedTime = 0;
        [UdonSynced] private int activePlayerID;

        public void FixedUpdate()
        {
            debugTextComponant.text = debugText + $"\n sync count {syncCount} \n UpdateCount {initializeSystemObject.UserUpdateCount()}";
            
            if (globalSync)
            {
                globalSyncUpdateWaitCount++;
                if (globalSyncUpdateWaitCount >= initializeSystemObject.UserUpdateCount() * SyncWaitSeconds)
                {
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "StartFirstSync");
                    globalSyncUpdateWaitCount = 0;
                    globalSync = false;
                }
                debugTextComponant.text = debugText + $"\n end";
                return;
            }
            
            /// 追加

            if (activePlayerID.Equals(Networking.LocalPlayer.playerId))
            {
                if (IsSyncTiming())
                {
                    this.Sync();
                }
                return;
            }
            
            if (localSync)
            {
                if (isPlaying)
                {
                    if (!videoPlayer.IsPlaying)
                    {
                        videoPlayer.Play();
                    }
                    localSync = false;
                    this.Sync();
                }
                return;
            }

            if (joinSync)
            {
                joinSyncUpdateWaitCount++;
                if (joinSyncUpdateWaitCount >= 30)
                {
                    if (!isPlaying)
                    {
                        if (joinSyncCount >= 10)
                        {
                            joinSync = false;
                        }
                        joinSyncUpdateWaitCount = 0;
                        joinSyncCount++;
                        return;
                    }
                    videoPlayer.LoadURL(syncedURL);

                    joinSyncUpdateWaitCount = 0;
                    joinSync = false;
                    localSync = true;
                    return;
                }

                return;
            }

            if (IsSyncTiming())
            {
                this.Sync();;
            }
            
        }
        
        public void OnURLChanged()
        {
            if (String.IsNullOrEmpty(url.GetUrl().Get()))
            {
                return;
            }

            if (!Networking.IsOwner(Networking.LocalPlayer, this.gameObject))
            {
                Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
            }

            syncedURL = url.GetUrl();
            globalSync = true;
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "URLInputFieldSetInactibe");

        }
        
        public void URLInputFieldSetInactibe()
        {
            inputFieldLoadMessage.SetActive(true);
            videoLoadErrorController.hide();
        }

        public void StartFirstSync()
        {
            if (Networking.IsOwner(Networking.LocalPlayer, this.gameObject))
            {
                videoPlayer.Stop();
                videoPlayer.LoadURL(syncedURL);
                isPlaying = true;
                return;
            }

            if (String.IsNullOrEmpty(syncedURL.Get()))
            {
                return;
            }
            videoPlayer.Stop();
            videoPlayer.LoadURL(syncedURL);
        }
        
        public override void OnVideoReady()
        {
            if (Networking.IsOwner(Networking.LocalPlayer, this.gameObject))
            {
                videoPlayer.Play();
                return;
            }

            if (isPlaying)
            {
                videoPlayer.Play();
                // 変更
                return;
            }
            // 変更
            localSync = true;
            
        }

        public override void OnVideoStart()
        {

            if (isPause)
            {
                isPause = false;
                isPlaying = true;
                return;
            }

            if (!isStream)
            {
                int videoTimeSeconds = (int) (videoPlayer.GetDuration());
                videoTime[2] = ((int) videoTimeSeconds / 60) / 60;
                videoTime[1] = ((int) videoTimeSeconds / 60 ) - (videoTime[2] * 60) ; // minute
                videoTime[0] = videoTimeSeconds - (videoTime[2] * 60 * 60) - (videoTime[1] * 60); // seconds// hour

                sliderController.SetSliderLength(videoPlayer.GetDuration());
            }
            
            inputFieldLoadMessage.SetActive(false);
            videoLoadErrorController.hide();

            float time = 0;
            if (Networking.IsOwner(Networking.LocalPlayer, this.gameObject))
            {
                videoStartGlobalTime = (float) Networking.GetServerTimeInSeconds();
                isPlaying = true;
                activePlayerID = Networking.LocalPlayer.playerId;
                
                var rawURL = syncedURL.Get();
                if (rawURL.Contains("t="))
                {
                
                    var raw = rawURL.Substring(rawURL.IndexOf("t="));
                    if (raw.Contains("&"))
                    {
                        raw = raw.Substring(0,raw.IndexOf('&'));
                    
                    }

                    time = Convert.ToSingle(raw.Replace("t=", "").Replace("s", ""));
                    SetVideoTime(Convert.ToSingle(time));
                }  
                
                debugText =
                    $"Playing URL \n {syncedURL} \n, Play? {isPlaying}, \n" +
                    $"\n ?t = {time}, now owner {Networking.GetOwner(this.gameObject).displayName}\n" +
                    $"";
                
                return;
            }
            
            // 変更
            //videoPlayer.Pause();
            localSync = true;

        }
        
        //********** Sync **********//

        public bool IsSyncTiming()
        {
            if (!isPlaying)
            {
                return false;
            }

            if (Time.realtimeSinceStartup - lastSyncTime >= syncFrequency)
            {
                
                return true;
            }

            return false;
        }

        public void SyncForUnityHierarchy()
        {
            localSync = true;
        }
        
        public void Sync()
        {
            lastSyncTime = Time.realtimeSinceStartup;
            float globalVideoTime = Mathf.Clamp((float) Networking.GetServerTimeInSeconds() - videoStartGlobalTime, 0, videoPlayer.GetDuration());
            if (Mathf.Clamp(Mathf.Abs(videoPlayer.GetTime() - globalVideoTime), 0, videoPlayer.GetDuration()) > deleyLimit)
            {
                videoPlayer.SetTime(globalVideoTime);
            }
            syncCount++;
        }
        
        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (Networking.LocalPlayer == player)
            {
                joinSync = true;
            }
            
        }
        
        public override void OnVideoEnd()
        {
            if (Networking.IsOwner(Networking.LocalPlayer, this.gameObject))
            {
                videoStartGlobalTime = (float) Networking.GetServerTimeInSeconds();;
                isPlaying = false;
                videoPlayer.Stop();
                syncedURL = VRCUrl.Empty;
            }
        }

        public override void OnVideoError(VideoError videoError)
        {
            videoLoadErrorController.show(videoError);
            inputFieldLoadMessage.SetActive(false);
        }
        
        public void SetVideoTime(float seconds)
        {
            if (!Networking.IsOwner(Networking.LocalPlayer, this.gameObject))
            {
                Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
            }
            
            videoStartGlobalTime += videoPlayer.GetTime() - seconds;
            videoPlayer.SetTime(Mathf.Clamp((float) Networking.GetServerTimeInSeconds() - videoStartGlobalTime, 0, videoPlayer.GetDuration()));
            videoStartGlobalTime += 0.02f;
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Sync");
            localSync = true;
        }

        public void OwnerPause()
        {
            if (!Networking.IsOwner(Networking.LocalPlayer, this.gameObject))
            {
                Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
            }

            if (isPause)
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "PauseExit");
                
                return;
            }
            videoPlayer.Pause();
            pausedTime = (float) Networking.GetServerTimeInSeconds();
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "GlobalPause");
            isPlaying = false;
            isPause = true;
        }

        public void GlobalPause()
        {
            videoPlayer.Pause();
        }

        public void PauseExit()
        {
            videoStartGlobalTime += (float) Networking.GetServerTimeInSeconds() - pausedTime;
            videoPlayer.Play();
            videoPlayer.SetTime(Mathf.Clamp((float) Networking.GetServerTimeInSeconds() - videoStartGlobalTime, 0, videoPlayer.GetDuration()));
            videoStartGlobalTime += 0.02f;
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Sync");
            localSync = true;
        }

        public void Reset()
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "ResetGlobal");
            if (!Networking.IsOwner(Networking.LocalPlayer, this.gameObject))
            {
                Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
            }
            
            syncedURL = VRCUrl.Empty;
            isPlaying = false;
            localSync = false;

        }
        
        public void ResetGlobal()
        {
            videoPlayer.Stop();
            
        }

        public int[] GetVideoTime()
        {
            return videoTime;
        }
        
        
    }
}