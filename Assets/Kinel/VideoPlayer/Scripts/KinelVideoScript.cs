using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Components;
using VRC.SDK3.Components.Video;
using VRC.SDK3.Video.Components.Base;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

namespace Kinel.VideoPlayer.Scripts
{
    public class KinelVideoScript : UdonSharpBehaviour
    {

        [SerializeField] private VideoTimeSliderController sliderController;
        [SerializeField] private VRCUrlInputField url;
        [SerializeField] private VideoLoadErrorController videoLoadErrorController;
        [SerializeField] private CountDown globalProcess, localProcess;
        [SerializeField] private ModeChanger modeChanger;
        [SerializeField] private GameObject inputFieldLoadMessage;
        [SerializeField] private float syncFrequency = 3f, deleyLimit = 1f;

        private BaseVRCVideoPlayer videoPlayer;
        private float lastSyncTime;
        private int localVideoID = 0;
        private int localPlayMode = 0;
        private bool isReady = false;
        private bool isPauseLocal = false;
        
        [NonSerialized]public bool masterOnlyLocal = false;
        [UdonSynced] public bool masterOnly = false;
        
        //[SerializeField] private Animator playbackSpeed;
        //[SerializeField] private AnimationClip[] clips;

        [UdonSynced] private VRCUrl syncedURL = VRCUrl.Empty;
        [UdonSynced] private bool isPlaying = false;
        [UdonSynced] private bool isPause = false;
        [UdonSynced] private float videoStartGlobalTime = 0;
        [UdonSynced] private float pausedTime = 0;
        [UdonSynced] private int globalVideoID = 0;
        [UdonSynced] private int globalPlayMode = 0;
        
        private const int VIDEO_MODE = 0;
        private const int STREAM_MODE = 1;

        public void Start()
        {
            
        }

        public void FixedUpdate()
        {
            if (IsSyncTiming())
                Sync();;
        }

        public void OnDisable()
        {
            if(videoPlayer != null)
                videoPlayer.Stop();
        }

//        public void SpeedChange(float speed)
//        {
//            if (!playbackSpeed.enabled)
//                playbackSpeed.enabled = true;
//            
//            playbackSpeed.SetFloat("PlaybackSpeed", speed);
//            playbackSpeed.SetBool("IsChange", true);
//        }
        
        public void OnURLChanged()
        {
            if (String.IsNullOrEmpty(url.GetUrl().Get()))
                return;

            if (masterOnly && !Networking.LocalPlayer.isMaster)
                return;
            
            Debug.Log($"[KineL] Start process");
            ChangeOwner(Networking.LocalPlayer);
            if (url.GetUrl().Get().Length >= 85)
            {
                videoLoadErrorController.showMessage("Input Error. Please check your URL length.");
                return;
            }
            syncedURL = url.GetUrl();
            PlayVideo(syncedURL);
        }

        public bool PlayVideo(VRCUrl playURL)
        {
            if (masterOnly && !Networking.LocalPlayer.isMaster)
                return false;
            
            if (!IsValidURL(playURL.Get()))
            {
                OnVideoError(VideoError.InvalidURL);
                Debug.Log($"[KineL] Loading Error: URL invalid");
                return false;
            } 
            
            if(videoPlayer.IsPlaying)
                videoPlayer.Stop();

            if (isPause)
            {
                isPauseLocal = false;
            }

            inputFieldLoadMessage.SetActive(true);
            videoLoadErrorController.hide();

            if (Networking.IsOwner(Networking.LocalPlayer, this.gameObject))
            {
                globalVideoID++;
                localVideoID = globalVideoID;
                isPause = false;
                isPlaying = true;
                syncedURL = playURL;
                url.SetUrl(VRCUrl.Empty);

                Debug.Log($"[KineL] Loading ... : now syncedURL {syncedURL}");
                localProcess.StartSyncWaitCountdown(1.5f, nameof(OwnerPlayVideo), false);
                return true;
            }
            Debug.Log($"[KineL] Loading ... : now syncedURL {syncedURL}");
            videoPlayer.LoadURL(playURL);
            isPauseLocal = false;
            return true;
        }

        // 時間調整用
        public void OwnerPlayVideo()
        {
            videoPlayer.LoadURL(syncedURL);
        }

        public override void OnVideoReady()
        {
            Debug.Log($"[KineL] Video ready # {syncedURL.Get()}");
            
            inputFieldLoadMessage.SetActive(false);
            videoLoadErrorController.hide();
            sliderController.SetSliderLength(videoPlayer.GetDuration());
            
            if (Networking.IsOwner(Networking.LocalPlayer, this.gameObject))
            {
                videoPlayer.Play();
                return;
            }

            if (isPlaying && !videoPlayer.IsPlaying)
                videoPlayer.Play();

            if (isPause)
                Sync();
        }

        public override void OnVideoStart()
        {
            Debug.Log($"[KineL] video start.");
            if (isPauseLocal)
            {
                isPauseLocal = false;
                return;
            }

            if (globalPlayMode == STREAM_MODE)
                return;
            
            
            if (Networking.IsOwner(Networking.LocalPlayer, this.gameObject))
            {
                float time = 0;
                videoStartGlobalTime = (float) Networking.GetServerTimeInSeconds();

                var rawURL = syncedURL.Get();
                
                //urlConverter.ConvertURL(rawURL);
                if (rawURL.Contains("youtube.com")
                    || rawURL.Contains("youtu.be"))
                {
                    var variables = rawURL.Split('&');
                    for (int i = 1; i < variables.Length; i++)
                    {
                        Debug.Log(variables[i]);
                        var parameter = variables[i];
                        if (parameter.Contains("list=") || parameter.Contains("index="))
                            continue;
                            
                        if (parameter.Contains("t="))
                        {
                            var str = parameter.Replace("t=", "").Replace("s", "");
                            int t = 0;
                            if (!int.TryParse(str, out t))
                                continue;
                            SetVideoTime(t);
                        }
                    }
                }

            }
        }
        
        public override void OnVideoEnd()
        {
            if (Networking.IsOwner(Networking.LocalPlayer, this.gameObject))
            {
                videoStartGlobalTime = (float) Networking.GetServerTimeInSeconds();
                isPlaying = false;
                videoPlayer.Stop();
                syncedURL = VRCUrl.Empty;
            }

            isPause = false;
        }

        public override void OnVideoLoop()
        {
            if (Networking.IsOwner(Networking.LocalPlayer, this.gameObject))
                videoStartGlobalTime = (float) Networking.GetServerTimeInSeconds();

        }

        public override void OnVideoError(VideoError videoError)
        {
            videoLoadErrorController.show(videoError);
            inputFieldLoadMessage.SetActive(false);
        }

        //********** Sync **********//

        public bool IsSyncTiming()
        {
            if (!isPlaying || globalPlayMode == STREAM_MODE)
                return false;
            
            if (Time.realtimeSinceStartup - lastSyncTime >= syncFrequency)
                return true;

            return false;
        }

        private float coolTime = 0;

        public override void OnDeserialization()
        {
            if (Networking.IsOwner(this.gameObject))
                return;

            coolTime += Time.deltaTime;

            if (masterOnlyLocal != masterOnly)
            {
                modeChanger.ToggleMasterOnly();
                Debug.Log("[Kinel] MasterOnly");
            }
                

            // play mode change
            if (localPlayMode != globalPlayMode)
            {
                if (coolTime <= 1)
                    return;
    
                Debug.Log("[Kinel] video mode synced.");
                coolTime = 0;
                localPlayMode = globalPlayMode;
                modeChanger.ChangeMode(globalPlayMode);
                return;
            }

            if (globalVideoID == localVideoID)
                return;

            localVideoID = globalVideoID;
            Debug.Log($"[KineL] Synced. # {syncedURL.Get()}");
            videoPlayer.Stop();
            PlayVideo(syncedURL);
        }



        //public void Paste()
        //{
            //var str = GUIUtility.systemCopyBuffer;
            //url.SetUrl(str);
            //url.ForceLabelUpdate();
       // }

        public void Sync()
        {
            lastSyncTime = Time.realtimeSinceStartup;
            float globalVideoTime = Mathf.Clamp((float) Networking.GetServerTimeInSeconds() - videoStartGlobalTime, 0, videoPlayer.GetDuration());
            if (Mathf.Clamp(Mathf.Abs(videoPlayer.GetTime() - globalVideoTime), 0, videoPlayer.GetDuration()) > deleyLimit)
                videoPlayer.SetTime(globalVideoTime);
        }

        public void Resync()
        {
            if (localVideoID == globalVideoID && isPlaying && !videoPlayer.IsPlaying)
            {
                Debug.Log("[Kinel] Resynce");
                PlayVideo(syncedURL);
                return;
            }
            Sync();
        }

        public void Reload()
        {
            Debug.Log("[Kinel] Reloading...");
            var lastVideoURL = syncedURL;
            ResetGlobal();
            PlayVideo(lastVideoURL);
            Debug.Log($"Reload completed. # {syncedURL}");
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (Networking.LocalPlayer == player)
            {
                modeChanger.ChangeMode(VIDEO_MODE);
                isReady = true;
            }
        }
        

        public void GlobalChangeMode(int mode)
        {
            if (!Networking.IsOwner(Networking.LocalPlayer, gameObject))
                ChangeOwner(Networking.LocalPlayer);

            globalPlayMode = mode;
            localPlayMode = mode;
        }

        public void SetVideoTime(float seconds)
        {
            ChangeOwner(Networking.LocalPlayer);
            videoStartGlobalTime += videoPlayer.GetTime() - seconds;
            videoPlayer.SetTime(Mathf.Clamp((float) Networking.GetServerTimeInSeconds() - videoStartGlobalTime, 0, videoPlayer.GetDuration()));
            videoStartGlobalTime += 0.02f;
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(Sync));
        }

        public void OwnerPause()
        {
            ChangeOwner(Networking.LocalPlayer);
            if (isPause)
            {
                SendCustomNetworkEvent(NetworkEventTarget.All, nameof(PauseExit));
                isPlaying = true;
                isPause = false;
                videoStartGlobalTime += (float) Networking.GetServerTimeInSeconds() - pausedTime;
                pausedTime = 0;
                return;
            }
            videoPlayer.Pause();
            pausedTime = (float) Networking.GetServerTimeInSeconds();
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(GlobalPause));
            isPlaying = false;
            isPause = true;
            isPauseLocal = true;
        }

        public void GlobalPause()
        {
            Debug.Log("[Kinel] Paused.");
            videoPlayer.Pause();
            isPauseLocal = true;
        }

        public void PauseExit()
        {
            videoPlayer.Play();
        }

        public void ResetGlobal()
        {
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(ResetLocal));
            ChangeOwner(Networking.LocalPlayer);
            url.SetUrl(VRCUrl.Empty);
            syncedURL = VRCUrl.Empty;
            isPlaying = false;
            isPause = false;
            videoStartGlobalTime = 0;
            pausedTime = 0;
        }

        public void ResetLocal()
        {
            videoPlayer.Stop();
            videoPlayer.LoadURL(VRCUrl.Empty);
            inputFieldLoadMessage.SetActive(false);
            videoLoadErrorController.hide();
            url.SetUrl(VRCUrl.Empty);
            isPauseLocal = false;
            
        }
        
        public void ResetForButton()
        {
            ResetGlobal();
        }

        public BaseVRCVideoPlayer GetVideoPlayer()
        {
            return videoPlayer;
        }

        public int GetPlayMode()
        {
            return localPlayMode;
        }

        public int GetGlobalPlayMode()
        {
            return globalPlayMode;
        }

        public void SetGlobalPlayMode(int playMode)
        {
            if(Networking.IsOwner(Networking.LocalPlayer, this.gameObject))
                ChangeOwner(Networking.LocalPlayer);

            globalPlayMode = playMode;
        }

        public void SetVideoInstance(BaseVRCVideoPlayer instance)
        {
            videoPlayer = instance;
        }

        private void ChangeOwner(VRCPlayerApi player)
        {
            if (!Networking.IsOwner(player, this.gameObject))
                Networking.SetOwner(player, this.gameObject);
            
        }
     
        public bool IsPlaying()
        {
            return isPlaying;
        }

        public bool IsPause()
        {
            return isPause;
        }

        public bool IsReady()
        {
            return isReady;
        }

        private bool IsValidURL(String str)
        {
            return str.Contains("https://") || str.Contains("rtspt://") || str.Contains("http://");
        }

        public VRCUrl GetUrl()
        {
            return syncedURL;
        }

        public ModeChanger GetModeChangeInstance()
        {
            return modeChanger;
        }

    }
}