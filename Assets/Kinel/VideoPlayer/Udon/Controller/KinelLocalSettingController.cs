using UdonSharp;
using UnityEngine;

namespace Kinel.VideoPlayer.Udon.Controller
{
    public class KinelLocalSettingController : UdonSharpBehaviour
    {
        [SerializeField] private KinelVideoPlayerUI videoPlayerUI;
        [SerializeField] private Animator uiAnimator;
        
        private bool mirrorInversion = false;
        
        public void Start()
        {
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
            Debug.Log($"SCRENN MODULE {videoPlayerUI.GetVideoPlayer().GetKinelScreenModules().Length}");
            foreach (var screen in videoPlayerUI.GetVideoPlayer().GetKinelScreenModules())
            {
                screen.SetMirrorInversion(!mirrorInversion);
                mirrorInversion = !mirrorInversion;
            }
        }
        
    }
}