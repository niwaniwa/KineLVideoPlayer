using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;

namespace Kinel.VideoPlayer.Udon.Controller
{
    public class KinelLocalSettingController : UdonSharpBehaviour
    {
        [SerializeField] private KinelVideoPlayerUI videoPlayerUI;
        [SerializeField] private Animator uiAnimator;
        
        [SerializeField] private bool mirrorInversion = false; // true = 反転 , false = そのまま
        [SerializeField] private Toggle mirrorInversionToggle;
        
        public void Start()
        {
            Initialize();
        }

        public void Initialize()
        {
            if (mirrorInversionToggle == null)
            {
                mirrorInversionToggle = gameObject.transform.Find("Local/Mirror").GetComponent<Toggle>();
            }

            SendCustomEventDelayedSeconds(nameof(InitializeMirror), 2f);
        }
        
        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (Networking.LocalPlayer.Equals(player))
            {
                Initialize();
            }
        }

        public void OnResyncClick()
        {
            videoPlayerUI.GetVideoPlayer().Sync();
        }

        public void OnReloadClick()
        {
            videoPlayerUI.GetVideoPlayer().Reload();
        }

        public void OnToggleMirrorInversion()
        {
            mirrorInversion = !mirrorInversion;
            ToggleMirrorInversion();
        }
        
        public void ToggleMirrorInversion()
        {
            var videoPlayer = videoPlayerUI.GetVideoPlayer();
            var screens = videoPlayer != null ? videoPlayer.GetKinelScreenModules() : null;
            if (screens == null) return;
            foreach (var screen in screens)
            {
                screen.SetMirrorInversion(!screen.IsMirrorInversion);
            }
        }

        public void InitializeMirror()
        {
            mirrorInversion = !mirrorInversion;
            mirrorInversionToggle.isOn = mirrorInversion;
        }
        
    }
}