using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.Udon;

namespace Kinel.VideoPlayer.Scripts
{
    public class VideoPlayerTimeTextUpdater : UdonSharpBehaviour
    {

        public KinelVideoScript kinelVideoPlayer;
        public VideoTimeSliderController sliderController;
        public Text endTime;
        public Text nowTime;
        public void FixedUpdate()
        {
            if (!kinelVideoPlayer.GetVideoPlayer())
                return;
            
            if (!sliderController.IsDrag() /*&& kinelVideoPlayer.IsReady()*/)
            {
                UpdateVideoTimeText((int) (kinelVideoPlayer.GetVideoPlayer().GetTime()));
            }
        }
        
        private void UpdateVideoTimeText(int videoTimeSeconds)
        {
            bool b = kinelVideoPlayer.GetVideoPlayer().GetDuration() > 3600;
            var now = TimeSpan.FromSeconds(videoTimeSeconds).ToString(b ? "hh\\:mm\\:ss" : "mm\\:ss");
            var videoLength = TimeSpan.FromSeconds(kinelVideoPlayer.GetVideoPlayer().GetDuration()).ToString(b ? "hh\\:mm\\:ss" : "mm\\:ss");
            nowTime.text = now;
            endTime.text = videoLength;

        }

        public void UpdateUserSelectSliderTime()
        {
            UpdateVideoTimeText((int) sliderController.slider.value);
        }
        
        
    }
}