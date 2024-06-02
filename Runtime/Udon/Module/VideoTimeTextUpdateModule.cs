using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;

namespace Kinel.VideoPlayer.Udon.Module
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class VideoTimeTextUpdateModule : KinelModule
    {

        [SerializeField] private KinelVideoPlayerUI videoPlayerUI;
        [SerializeField] private VideoSeekModule seekModule;
        [SerializeField] private string format;
        [SerializeField] private Text videoTimeText, nowVideoTimeText;
        [SerializeField] private float updateInterval;

        private float updateTime = 50;

        public void Start()
        {
            UpdateVideoText(0, 0);
            videoPlayerUI.RegisterListener(this);
        }

        public void FixedUpdate()
        {

            // if (Networking.GetServerTimeInMilliseconds() - updateTime <= updateInterval)
            // {
            //     Debug.Log($"RETURN; updateTime {updateTime}, mainasu {Networking.GetServerTimeInMilliseconds() - updateTime}");
            //     return;
            // }
            
            if (!videoPlayerUI.GetVideoPlayer().IsPlaying)
                return;

            if (!videoPlayerUI.IsVideo())
                return;

            // if (!seekModule.IsDrag())
            // {
            //     // updateTime = Networking.GetServerTimeInMilliseconds();
            //     UpdateVideoTimeText(videoPlayerUI.GetSystem().GetTime());
            // }
            UpdateVideoTimeText(seekModule.IsDrag() ? (int) seekModule.GetSeekSlider().value : videoPlayerUI.GetSystem().GetTime());
        }
        
        public void UpdateVideoTimeText(float videoTimeSeconds)
        {
            nowVideoTimeText.text = TimeSpan.FromSeconds(videoTimeSeconds).ToString(format);
        }
        
        public void UpdateVideoText(float videoTimeSeconds, float duration)
        {
            UpdateVideoTimeText(videoTimeSeconds);
            videoTimeText.text = TimeSpan.FromSeconds(duration).ToString(format);
        }

        public void UpdateCustomText(String now, String videoTime)
        {
            nowVideoTimeText.text = now;
            videoTimeText.text = videoTime;
        }
        
        public void UpdateUserSelectSliderTime()
        {
            // UpdateVideoTimeText((int) seekModule.GetSeekSlider().value);
        }

        public override void OnKinelVideoStart()
        {
            
            if (!videoPlayerUI.IsVideo()){
                UpdateCustomText("LIVE", "LIVE");
                return;
            }
            UpdateVideoText(0, videoPlayerUI.GetSystem().GetDuration());
        }

        public override void OnKinelChangeVideoTime()
        {
            if (!videoPlayerUI.IsVideo())
                return;
            
            // Debug.Log($"{videoPlayerUI.GetVideoPlayer().VideoTime}");
            UpdateVideoTimeText(videoPlayerUI.GetVideoPlayer().VideoTime);
        }

        public override void OnKinelVideoReset()
        {
            UpdateVideoText(0, 0);
        }

        // public void OnKinelVideoModeChange()
        // {
        //     if (!videoPlayerUI.IsVideo())
        //     {
        //         UpdateCustomText("LIVE", "LIVE");
        //         return;
        //     }
        //     UpdateVideoText(0, 0);
        // }


    }
}