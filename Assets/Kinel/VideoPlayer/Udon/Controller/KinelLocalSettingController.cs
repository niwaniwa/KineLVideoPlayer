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

            mirrorInversionToggle.isOn = !mirrorInversion;
            SetMirrorInversion(mirrorInversion);
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

        public void ToggleMirrorInversion()
        {
            mirrorInversion = !mirrorInversion;
            SetMirrorInversion(mirrorInversion);
        }

        public void SetMirrorInversion(bool isMirrorInversion)
        {
            foreach (var screen in videoPlayerUI.GetVideoPlayer().GetKinelScreenModules())
                screen.SetMirrorInversion(isMirrorInversion);
            
        }
        
    }
}