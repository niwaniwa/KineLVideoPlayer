using UdonSharp;
using UnityEngine;
using UnityEngine.UI;

namespace Kinel.VideoPlayer.Udon.Module.Test
{
    public class TestModule : UdonSharpBehaviour
    {

        [SerializeField] private KinelVideoPlayerUI videoPlayerUI;

        [SerializeField] private Text text;

        public void Start()
        {
            videoPlayerUI.RegisterListener(this);
        }

        public void OnKinelVideoModeChange()
        {
            var mode = videoPlayerUI.GetVideoPlayer().GetCurrentVideoMode();
            switch (mode)
            {
                case 1:
                    text.text = "STREAM MODE";
                    break;
                default:
                    text.text = "VIDEO MODE";
                    break;
            }
        }

    }
}