using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.Core;
using VRC.SDK3.Components;
using VRC.SDK3.Components.Video;
using VRC.SDK3.Video.Components.AVPro;
using VRC.SDK3.Video.Components.Base;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;
using VRC.Udon.Wrapper.Modules;

namespace Kinel.VideoPlayer.Scripts
{
    public class KinelVideoScript : UdonSharpBehaviour
    {
        public BaseVRCVideoPlayer stream, video;
        public VideoTimeSliderController sliderController;
        public InitializeScript initializeSystemObject; 
        public VRCUrlInputField url;
        public VideoLoadErrorController videoLoadErrorController;
        public CountDown globalProcess, localProcess;
        public Toggle streamButton;
        public Text debugTextComponant;
        public GameObject inputFieldLoadMessage;
        

        private BaseVRCVideoPlayer videoPlayer;
        private float lastSyncTime, syncFrequency = 0.5f, deleyLimit = 0.5f;
        private int joinSyncActCount = 0;
        private int syncCount = 0;
        private int[] videoTime = new int[3];
        private string debugText;

        [UdonSynced] private bool isPlaying = false;
        [UdonSynced] private bool isPause = false;
        [UdonSynced] private VRCUrl syncedURL;
        [UdonSynced] private float videoStartGlobalTime = 0;
        [UdonSynced] private float pausedTime = 0;
        [UdonSynced] private float pauseElapsedTime = 0;
        [UdonSynced] private int activePlayerID;
        [UdonSynced] private bool isStreamGlobal = false;
        [UdonSynced] private bool isSync = false;
        

        public void Start()
        {
            videoPlayer = video;
        }
        
        public void FixedUpdate()
        {
            debugTextComponant.text = $"isStream : {isStreamGlobal} \n UserUpdateCount {initializeSystemObject.UserUpdateCount()} " +
                                      $"\n isPlaying(global) {isPlaying}" +
                                      $"\n isPlaying(local)  {video.IsPlaying}";
            
            if (IsSyncTiming())
            {
                this.Sync();;
            }
        }
        
        public void OnURLChanged()
        {
            if (String.IsNullOrEmpty(url.GetUrl().Get()) || isSync)
            {
                return;
            }

            ChangeOwner(Networking.LocalPlayer);
            isSync = true;
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
                isSync = false;
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

            if (isPlaying && !videoPlayer.IsPlaying)
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
            
            inputFieldLoadMessage.SetActive(false);
            videoLoadErrorController.hide();
            
            if (isStreamGlobal)
            {
                videoTime = new int[] {0, 0, 0};
                isPlaying = true;
                return;
            }
            
            int videoTimeSeconds = isStreamGlobal ? 0 : (int) (videoPlayer.GetDuration());
            videoTime[2] = (videoTimeSeconds / 60) / 60;
            videoTime[1] = (videoTimeSeconds / 60 ) - (videoTime[2] * 60) ; // minute
            videoTime[0] = videoTimeSeconds - (videoTime[2] * 60 * 60) - (videoTime[1] * 60); // seconds// hour
            sliderController.SetSliderLength(videoPlayer.GetDuration());
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
            localProcess.StartSyncWaitCountdown(2, "LocalSync", false);
        }
        
        //********** Sync **********//

        public bool IsSyncTiming()
        {
            if (!isPlaying || isStreamGlobal)
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
            if (!videoPlayer.IsPlaying && isPlaying)
            {
                videoPlayer.LoadURL(syncedURL);
            }
        }
        
        public void JoinSync()
        {
            streamButton.isOn = isStreamGlobal;
            videoPlayer = (isStreamGlobal ? stream : video);
            sliderController.Freeze();
            if (!isPlaying)
            {
                if (joinSyncActCount >= 3)
                {
                    isSync = false;
                    return;
                }

                var isSuccess = localProcess.StartSyncWaitCountdown(1, "JoinSync", false);
                joinSyncActCount++;
                return;
            }

            if (String.IsNullOrEmpty(syncedURL.Get()))
            {
                localProcess.StartSyncWaitCountdown(1, "JoinSync", false);
                return;
            }
            isSync = false;
            videoPlayer.LoadURL(syncedURL);

        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            videoPlayer = video;
            if (Networking.LocalPlayer == player)
            {
                isSync = true;
                localProcess.StartSyncWaitCountdown(10, "JoinSync", false);
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
            
            localProcess.StartSyncWaitCountdown(20, "LocalSync", false);
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
            inputFieldLoadMessage.SetActive(false);
            videoLoadErrorController.hide();
            url.SetUrl(VRCUrl.Empty);
        }

        public int[] GetVideoTime()
        {
            return videoTime;
        }

        public BaseVRCVideoPlayer GetVideoPlayer()
        {
            return videoPlayer ?? video;
        }

        public void SetStreamModeFlag(bool isStream)
        {
            this.isStreamGlobal = isStream;
        }

        private void ChangeOwner(VRCPlayerApi player)
        {
            if (!Networking.IsOwner(player, this.gameObject))
            {
                Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
            }
        }

        public bool IsStream()
        {
            return isStreamGlobal;
        }

        public bool IsPlaying()
        {
            return isPlaying;
        }

        public bool IsPause()
        {
            return isPause;
        }
        
    }
}