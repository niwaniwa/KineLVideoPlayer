using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Kinel.VideoPlayer.Udon.Controller
{
    
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class KinelGlobalSettingController : UdonSharpBehaviour
    {

        [SerializeField] private KinelVideoPlayerUI videoPlayerUI;
        [SerializeField] private Animator uiAnimator;
        [SerializeField] private GameObject loopActive, loopDisable;
        [SerializeField] private GameObject[] lockToggleObjects;
        
        public const int VIDEO_MODE = 0;
        public const int STREAM_MODE = 1;
        
        public void Start()
        {
            videoPlayerUI.RegisterListener(this);
            SendCustomEventDelayedFrames(nameof(ToggleLoopIcon), 5);
        }

        public void OnResetClick()
        {
            videoPlayerUI.GetVideoPlayer().ResetGlobal();
        }

        public void OnReloadClick()
        {
            videoPlayerUI.GetVideoPlayer().ReloadGlobal();
        }

        public void OnLoopClick()
        {
            if (!Networking.IsOwner(Networking.LocalPlayer, videoPlayerUI.GetVideoPlayer().gameObject))
                return;
            
            videoPlayerUI.GetVideoPlayer().Loop = !videoPlayerUI.GetVideoPlayer().Loop;
            ToggleLoopIcon();
            
        }

        public void OnLockChangeToggle()
        {
            if (!Networking.LocalPlayer.isMaster)
                return;

            var videoPlayer = videoPlayerUI.GetVideoPlayer();
            videoPlayer.TakeOwnership();
            videoPlayer.IsLock = !videoPlayer.IsLock;
            videoPlayer.RequestSerialization();

        }

        public void OnModeChange()
        {
            videoPlayerUI.GetVideoPlayer().TakeOwnership();
            switch (videoPlayerUI.GetVideoPlayer().GetCurrentVideoMode())
            {
                case VIDEO_MODE:
                    videoPlayerUI.GetVideoPlayer().ChangeMode(STREAM_MODE);
                    break;
                case STREAM_MODE:
                    videoPlayerUI.GetVideoPlayer().ChangeMode(VIDEO_MODE);
                    break;
                default:
                    break;
            }
        }

        public void OnKinelLoopChanged()
        {
            ToggleLoopIcon();
        }

        public void OnKinelVideoPlayerLocked()
        {
            uiAnimator.SetBool("MasterLock", true);
            
            if (Networking.LocalPlayer.isMaster)
                return;
            
            foreach (var target in lockToggleObjects)
            {
                target.SetActive(!target.activeSelf);
            }
            
        }

        public void OnKinelVideoPlayerUnlocked()
        {
            uiAnimator.SetBool("MasterLock", false);
            
            if (Networking.LocalPlayer.isMaster)
                return;
            
            foreach (var target in lockToggleObjects)
            {
                target.SetActive(!target.activeSelf);
            }
        }

        public void OnKinelVideoModeChange()
        {
            uiAnimator.SetInteger("Mode", videoPlayerUI.GetVideoPlayer().GetCurrentVideoMode());
        }

        private void ToggleLoopIcon()
        {
            loopActive.SetActive(videoPlayerUI.GetVideoPlayer().Loop);
            loopDisable.SetActive(!videoPlayerUI.GetVideoPlayer().Loop);
        }
        
        
    }
}