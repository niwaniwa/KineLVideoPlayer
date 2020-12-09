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
        public Text timeText;
        public GameObject game;

        public void FixedUpdate()
        {
            if (!sliderController.IsDrag())
            {
                UpdateVideoTimeText((int) (kinelVideoPlayer.GetVideoPlayer().GetTime()));
            }
        }
        
        private void UpdateVideoTimeText(int videoTimeSeconds)
        {
            int hour = ((int) videoTimeSeconds / 60) / 60;
            int minute = ((int) videoTimeSeconds / 60 ) - (hour * 60);
            int seconds = videoTimeSeconds - (hour * 60 * 60 ) - (minute * 60);
            timeText.text =  (kinelVideoPlayer.GetVideoTime()[2] != 0 ? $"{hour:00} : " : "") + $"{minute:00} : {seconds:00} / " + (kinelVideoPlayer.GetVideoTime()[2] != 0 ? $" {kinelVideoPlayer.GetVideoTime()[2]:00} : " : "")
                            + $"{kinelVideoPlayer.GetVideoTime()[1]:00} : {kinelVideoPlayer.GetVideoTime()[0]:00}";
        }

        public void UpdateUserSelectSliderTime()
        {
            UpdateVideoTimeText((int) sliderController.slider.value);
        }
        
        
    }
}