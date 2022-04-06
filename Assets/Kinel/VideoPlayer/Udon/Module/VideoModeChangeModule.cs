using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Kinel.VideoPlayer.Udon.Module
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class VideoModeChangeModule : UdonSharpBehaviour
    {

        [SerializeField] private KinelVideoPlayerUI videoPlayerUI;

        private KinelVideoPlayer videoPlayer;
        
        private const int VIDEO_MODE = 0;
        private const int STREAM_MODE = 1;

        public void Start()
        {
            videoPlayer = videoPlayerUI.GetVideoPlayer();
        }

        public void ChangeMode()
        {
            Networking.SetOwner(Networking.LocalPlayer, videoPlayer.gameObject);
            switch (videoPlayer.GetCurrentVideoMode())
            {
                case VIDEO_MODE:
                    videoPlayer.ChangeMode(STREAM_MODE);
                    break;
                case STREAM_MODE:
                    videoPlayer.ChangeMode(VIDEO_MODE);
                    break;
                default:
                    break;
            }
        }


    }
}