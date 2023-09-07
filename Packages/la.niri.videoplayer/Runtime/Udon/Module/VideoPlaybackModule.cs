using System;
using UdonSharp;
using UnityEngine;

namespace Kinel.VideoPlayer.Udon.Module
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class VideoPlaybackModule : KinelModule
    {
        [SerializeField] private KinelVideoPlayerUI videoPlayerUI;
        [SerializeField] private GameObject pause, play;
        [SerializeField] private int additionalTime;

        public void Start() => videoPlayerUI.GetVideoPlayer().RegisterListener(this);
        
        public void TogglePause()
        {
            Debug.Log($"{DEBUG_PREFIX} {name} Toggle pause");
            videoPlayerUI.GetVideoPlayer().TakeOwnership();
            if (!videoPlayerUI.GetVideoPlayer().IsPause)
            {
                videoPlayerUI.GetVideoPlayer().Pause();
                Pause();
            }
            else
            {
                videoPlayerUI.GetVideoPlayer().Play();
                Play();
            }
        }

        private void Pause()
        {
            pause.SetActive(false);
            play.SetActive(true);
        }

        private void Play()
        {
            pause.SetActive(true);
            play.SetActive(false);
        }
        
        public void Forward()
        {
            float time = videoPlayerUI.GetVideoPlayer().VideoTime + additionalTime;
            if (time >= videoPlayerUI.GetSystem().GetDuration())
                time = videoPlayerUI.GetSystem().GetDuration();
            
            videoPlayerUI.GetVideoPlayer().SetVideoTime(time);
        }

        public void Rewind()
        {
            float time = videoPlayerUI.GetVideoPlayer().VideoTime - additionalTime;
            if (time < 0)
                time = 0;
            videoPlayerUI.GetVideoPlayer().SetVideoTime(time);
        }

        public void ResetVideoPlayer() => videoPlayerUI.GetVideoPlayer().ResetGlobal();

        public override void OnKinelVideoStart() => Play();

        public override void OnKinelVideoEnd() => Pause();
        
    }
}