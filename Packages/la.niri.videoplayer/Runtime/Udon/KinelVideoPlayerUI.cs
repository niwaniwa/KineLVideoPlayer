using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Video.Components.Base;

namespace Kinel.VideoPlayer.Udon
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class KinelVideoPlayerUI : UdonSharpBehaviour
    {
        
        [SerializeField] public KinelVideoPlayer videoPlayer;
        
        [SerializeField] public bool isAutoFill = false;
        private bool _isUIActive = true;

        public void Start()
        {
            if (!videoPlayer)
            {
                _isUIActive = false;
            }
        }

        public KinelVideoPlayer GetVideoPlayer()
        {
            return videoPlayer;
        }

        public BaseVRCVideoPlayer GetSystem()
        {
            return videoPlayer.GetVideoPlayerController().GetCurrentVideoPlayer();
        }

        public void RegisterListener(UdonSharpBehaviour udon)
        {
            videoPlayer.RegisterListener(udon);
        }

        public void UnregisterListener(UdonSharpBehaviour udon)
        {
            videoPlayer.UnregisterListener(udon);
        }

        public bool IsUIAcitve()
        {
            return _isUIActive;
        }

        public bool IsVideo()
        {
            return videoPlayer.IsVideo();
        }

    }
}