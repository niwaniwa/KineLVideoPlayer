using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.Core;
using VRC.SDK3.Components;
using VRC.SDK3.Components.Video;
using VRC.SDK3.Video.Components.Base;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;
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
        public CountDown globalProcess, localProcess;
        public Text debugTextComponant;
        public GameObject inputFieldLoadMessage;

        private float lastSyncTime, syncFrequency = 0.5f, deleyLimit = 0.5f;
        private int syncCount = 0;
        private int[] videoTime = new int[3];
        private string debugText;
        private bool isStream = false;

        [UdonSynced] public bool isPlaying = false;
        [UdonSynced] public bool isPause = false;
        
        [UdonSynced] private VRCUrl syncedURL;
        [UdonSynced] private float videoStartGlobalTime = 0;
        [UdonSynced] private float pausedTime = 0;
        [UdonSynced] private float pauseElapsedTime = 0;
        [UdonSynced] private int activePlayerID;

        public void FixedUpdate()
        {
            debugTextComponant.text = debugText + $"\n sync count {syncCount} \n UpdateCount {initializeSystemObject.UserUpdateCount()} + video time = {videoPlayer.GetDuration() == null}";

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

            ChangeOwner(Networking.LocalPlayer);

            syncedURL = url.GetUrl();
            globalProcess.StartSyncWaitCountdown(2, "GlobalSync", true);
            SendCustomNetworkEvent(NetworkEventTarget.All, "URLInputFieldSetInactibe");
        }
        
        public void URLInputFieldSetInactibe()
        {
            inputFieldLoadMessage.SetActive(true);
            videoLoadErrorController.hide();
        }

        public void GlobalSync()
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
                return;
            }

            localProcess.StartSyncWaitCountdown(2, "LocalSync", false);
            
        }

        public override void OnVideoStart()
        {

            if (isPause)
            {
                isPause = false;
                isPlaying = true;
                return;
            }
            
            isStream = (videoPlayer.GetDuration() == null);
            int videoTimeSeconds = isStream ? 0 : (int) (videoPlayer.GetDuration());
            videoTime[2] = isStream ? 0 : ((int) videoTimeSeconds / 60) / 60;
            videoTime[1] = isStream ? 0 : ((int) videoTimeSeconds / 60 ) - (videoTime[2] * 60) ; // minute
            videoTime[0] = isStream ? 0 : videoTimeSeconds - (videoTime[2] * 60 * 60) - (videoTime[1] * 60); // seconds// hour

            sliderController.SetSliderLength((isStream ? 0.1f : videoPlayer.GetDuration()));
            
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

                
                return;
            }
            
            // 変更
            //videoPlayer.Pause();
            localProcess.StartSyncWaitCountdown(2, "LocalSync", false);

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
            localProcess.StartSyncWaitCountdown(2, "LocalSync", false);
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

        public void LocalSync()
        {
            if (!videoPlayer.IsPlaying)
            {
                videoPlayer.Play();
            }
            this.Sync();
        }

        private int joinSyncActCount = 0;
        public void JoinSync()
        {
            if (!isPlaying)
            {
                if (joinSyncActCount >= 3)
                {
                    return;
                }
                localProcess.StartSyncWaitCountdown(5, "JoinSync", false);
                return;
            }
            videoPlayer.LoadURL(syncedURL);
            return;
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (Networking.LocalPlayer == player)
            {
                localProcess.StartSyncWaitCountdown(5, "JoinSync", false);
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
            ChangeOwner(Networking.LocalPlayer);
            
            videoStartGlobalTime += videoPlayer.GetTime() - seconds;
            videoPlayer.SetTime(Mathf.Clamp((float) Networking.GetServerTimeInSeconds() - videoStartGlobalTime, 0, videoPlayer.GetDuration()));
            videoStartGlobalTime += 0.02f;
            SendCustomNetworkEvent(NetworkEventTarget.All, "Sync");
            
            localProcess.StartSyncWaitCountdown(2, "LocalSync", false);
        }

        public void OwnerPause()
        {
            ChangeOwner(Networking.LocalPlayer);

            if (isPause)
            {
                SendCustomNetworkEvent(NetworkEventTarget.All, "PauseExit");
                
                return;
            }
            videoPlayer.Pause();
            pausedTime = (float) Networking.GetServerTimeInSeconds();
            SendCustomNetworkEvent(NetworkEventTarget.All, "GlobalPause");
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
            
            SendCustomNetworkEvent(NetworkEventTarget.All, "Sync");
            localProcess.StartSyncWaitCountdown(2, "LocalSync", false);
        }

        public void Reset()
        {
            SendCustomNetworkEvent(NetworkEventTarget.All, "ResetGlobal");
            ChangeOwner(Networking.LocalPlayer);
            
            syncedURL = VRCUrl.Empty;
            isPlaying = false;
        }
        
        public void ResetGlobal()
        {
            videoPlayer.Stop();
            
        }

        public int[] GetVideoTime()
        {
            return videoTime;
        }

        private void ChangeOwner(VRCPlayerApi player)
        {
            if (!Networking.IsOwner(player, this.gameObject))
            {
                Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
            }
        }
        
        
    }
}