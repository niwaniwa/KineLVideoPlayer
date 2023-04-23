using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;

namespace Kinel.VideoPlayer.Udon.Controller
{
    
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class KinelGlobalSettingController : UdonSharpBehaviour
    {

        [SerializeField] private KinelVideoPlayerUI videoPlayerUI;
        // [SerializeField] private Animator uiAnimator;
        [SerializeField] private GameObject loopActive, loopDisable;
        [SerializeField] private GameObject[] lockToggleObjects;
        [SerializeField] private Slider lockSlider, modeSlider;
        [SerializeField] private Image lockBackground, modeBackground;

        public void Start()
        {
            videoPlayerUI.RegisterListener(this);
            SendCustomEventDelayedFrames(nameof(ToggleLoopIcon), 5);
        }

        public void OnResetClick()
        {
            TakeOwnership();
            videoPlayerUI.GetVideoPlayer().ResetGlobal();
        }

        public void OnReloadClick()
        {
            TakeOwnership();
            videoPlayerUI.GetVideoPlayer().ReloadGlobal();
        }

        public void OnLoopClick()
        {
            TakeOwnership();
            videoPlayerUI.GetVideoPlayer().Loop = !videoPlayerUI.GetVideoPlayer().Loop;
            videoPlayerUI.GetVideoPlayer().RequestSerialization();
            ToggleLoopIcon();
           
        }

        public void OnLockChangeToggle()
        {
#if !UNITY_EDITOR
            if (!Networking.LocalPlayer.isMaster)
                return;
#endif
            TakeOwnership();
            var videoPlayer = videoPlayerUI.GetVideoPlayer();
            videoPlayer.IsLock = !videoPlayer.IsLock;
            videoPlayer.RequestSerialization();

        }

        public void OnModeChange()
        {
            TakeOwnership();
            switch (videoPlayerUI.GetVideoPlayer().GetCurrentVideoMode())
            {
                case (int) KinelVideoMode.Video:
                    videoPlayerUI.GetVideoPlayer().ChangeMode(KinelVideoMode.Stream);
                    break;
                case (int) KinelVideoMode.Stream:
                    videoPlayerUI.GetVideoPlayer().ChangeMode(KinelVideoMode.Video);
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
            // uiAnimator.SetBool("MasterLock", true);
            lockSlider.value = 0.8f;
            lockBackground.color = new Color(1f, 0.5f, 0.5f, 1.0f);
            
            if (Networking.LocalPlayer.isMaster)
                return;
            
            foreach (var target in lockToggleObjects)
            {
                target.SetActive(!target.activeSelf);
            }
            
        }

        public void OnKinelVideoPlayerUnlocked()
        {
            // uiAnimator.SetBool("MasterLock", false);
            lockSlider.value = 0.15f;
            lockBackground.color = new Color(1f, 1f, 1f, 1f);
            if (Networking.LocalPlayer.isMaster)
                return;
            
            foreach (var target in lockToggleObjects)
            {
                target.SetActive(!target.activeSelf);
            }
        }

        public void OnKinelVideoModeChange()
        {
            // uiAnimator.SetInteger("Mode", videoPlayerUI.GetVideoPlayer().GetCurrentVideoMode());

            if (videoPlayerUI.GetVideoPlayer().GetCurrentVideoMode() == (int) KinelVideoMode.Video)
            {
                modeSlider.value = 0.15f;
                modeBackground.color = new Color(1f, 1f, 1f, 1.0f);
            }
            else
            {
                modeSlider.value = 0.85f;
                modeBackground.color = new Color(1f, 0.5f, 0.5f, 1.0f);
            }
        }

        private void ToggleLoopIcon()
        {
            loopActive.SetActive(videoPlayerUI.GetVideoPlayer().Loop);
            loopDisable.SetActive(!videoPlayerUI.GetVideoPlayer().Loop);
        }

        private void TakeOwnership()
        {

            if (Networking.IsOwner(Networking.LocalPlayer, gameObject))
            {
                if (!Networking.IsOwner(Networking.LocalPlayer, videoPlayerUI.GetVideoPlayer().gameObject))
                {
                    videoPlayerUI.GetVideoPlayer().TakeOwnership();
                    return;
                }

                return;
            }
            
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            videoPlayerUI.GetVideoPlayer().TakeOwnership();
            
            
        }
        
        
    }
}