using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Kinel.VideoPlayer.Udon.Module
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class VideoModeChangeModule : KinelModule
    {
        [SerializeField] private KinelVideoPlayerUI videoPlayerUI;

        private KinelVideoPlayer videoPlayer;

        public void Start()
        {
            videoPlayer = videoPlayerUI.GetVideoPlayer();
        }

        public void ChangeMode()
        {
            Networking.SetOwner(Networking.LocalPlayer, videoPlayer.gameObject);
            switch (videoPlayer.GetCurrentVideoMode())
            {
                case (int) KinelVideoMode.Video:
                    videoPlayer.ChangeMode(KinelVideoMode.Stream);
                    break;
                case (int) KinelVideoMode.Stream:
                    videoPlayer.ChangeMode(KinelVideoMode.Video);
                    break;
                default:
                    break;
            }
        }


    }
}