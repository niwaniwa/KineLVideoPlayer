using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;

namespace Kinel.VideoPlayer.Scripts
{
    public class VideoTimeSliderController : UdonSharpBehaviour
    {

        public KinelVideoScript kinelVideoPlayer;
        public Slider slider;
        public InitializeScript initializeSystemObject;
        
        private const int waitTimeSeconds = 1; // seconds

        private bool isDrag = false;
        private bool isWait = false;
        private int waitCount = 0;

        public void OnSliderDrag()
        {
            isDrag = true;
        }

        public void OnSliderDrop()
        {
            kinelVideoPlayer.SetVideoTime(slider.value);
            isWait = true;
        }
        
        public void FixedUpdate()
        {
            if (isWait)
            {
                waitCount++;
                if (waitCount >= initializeSystemObject.UserUpdateCount() * waitTimeSeconds)
                {
                    isDrag = false;
                    isWait = false;
                    waitCount = 0;
                }

                return;
            }

            if (!isDrag)
            {
                slider.value = kinelVideoPlayer.videoPlayer.GetTime();
            }
            
        }

        public void SetSliderLength(float time)
        {
            this.slider.maxValue = time;
        }
    }
}