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

        private const int waitTimeSeconds = 1; // seconds

        private bool isDrag = false;
        private bool isWait = false;
        private float waitTiem = 0;
        private const int VIDEO_MODE = 0;
        private const int STREAM_MODE = 1;

        public void OnSliderDrag()
        {
            if ((kinelVideoPlayer.masterOnly && !Networking.LocalPlayer.isMaster))
                return;
                
            isDrag = true;
        }

        public void OnSliderDrop()
        {
            if (kinelVideoPlayer.GetGlobalPlayMode() == STREAM_MODE || (kinelVideoPlayer.masterOnly && !Networking.LocalPlayer.isMaster))
                return;
            
            kinelVideoPlayer.SetVideoTime(slider.value);
            isWait = true;
        }
        
        public void FixedUpdate()
        {
            
            if (!kinelVideoPlayer.GetVideoPlayer())
                return;
            
            if (kinelVideoPlayer.GetGlobalPlayMode() == STREAM_MODE)
                return;
            
            if (isWait)
            {
                waitTiem += Time.deltaTime;
                if (waitTiem >= waitTimeSeconds)
                {
                    isDrag = false;
                    isWait = false;
                    waitTiem = 0;
                }
                return;
            }

            if (!isDrag/* && kinelVideoPlayer.IsReady()*/)
                slider.value = kinelVideoPlayer.GetVideoPlayer().GetTime();
            
        }

        public void SetSliderLength(float time)
        {
            this.slider.maxValue = time;
        }

        public bool IsDrag()
        {
            return isDrag;
        }

        public void Freeze()
        {
            slider.interactable = false;
            slider.value = slider.maxValue;
        }

        public void UnFreeze()
        {
            slider.interactable = true;
        }

        public bool IsFreeze()
        {
            return slider.IsInteractable();
        }

        
    }
}