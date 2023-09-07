using UdonSharp;
using UnityEngine;
using UnityEngine.UI;

namespace Kinel.VideoPlayer.Udon.Module.Test
{
    public class KinelScreenDebugModule : UdonSharpBehaviour
    {

        public KinelVideoPlayer videoPlayer;

        public Toggle mirrorT, screenT;
        public Slider brightness, trasparency;
        
        // public void ToggleScreenInversion()
        // {
        //     foreach (var screen in videoPlayer.GetKinelScreenModules())
        //     {
        //         screen.SetScreenInversion(screenT.isOn);
        //     }
        // }
        
        public void ToggleMirrorInversion()
        {
            foreach (var screen in videoPlayer.GetKinelScreenModules())
            {
                screen.SetMirrorInversion(mirrorT.isOn);
            }
        }

        // public void SetTransparency()
        // {
        //     foreach (var screen in videoPlayer.GetKinelScreenModules())
        //     {
        //         screen.SetTransparency(trasparency.value);
        //     }
        // }
        
        public void SetBrightness()
        {
            foreach (var screen in videoPlayer.GetKinelScreenModules())
            {
                screen.SetBrightness(brightness.value);
            }
        }

    }
}