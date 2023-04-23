using Kinel.VideoPlayer.Udon.Controller;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;

namespace Kinel.VideoPlayer.Udon.Module
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class VideoSeekModule : KinelModule
    {

        [SerializeField] private KinelVideoPlayerUI videoPlayerUI;
        [SerializeField] private Slider slider;
        
        [SerializeField] private float updateInterval;

        private float updateTime = 50;

        private const int WAIT = 1; // seconds

        private KinelVideoPlayerController _controller;

        private bool _isLock = false;
        private bool _isDrag = false;
        private bool _isWait = false;
        private float _waitTiem = 0;
        private const int VIDEO_MODE = 0;
        private const int STREAM_MODE = 1;
        

        public void Start()
        {
            videoPlayerUI.RegisterListener(this);
            _controller = videoPlayerUI.GetVideoPlayer().GetVideoPlayerController();
        }

        public override void OnKinelVideoPlayerLocked()
        {
            _isLock = true;
            Lock();
        }

        public override void OnKinelVideoPlayerUnlocked()
        {
            _isLock = false;
            Unlock();
        }

        public override void OnKinelVideoReady()
        {
            if (_controller.CurrentMode == STREAM_MODE)
            {
                if (!videoPlayerUI.IsVideo())
                {
                    SetSliderLength(0);
                    Lock();
                    return;
                }
            }
            
            SetSliderLength(videoPlayerUI.GetVideoPlayer().GetVideoPlayerController().GetCurrentVideoPlayer().GetDuration());
        }

        public void OnSliderDrag()
        {
            _isDrag = true;
        }
        

        public void OnSliderDrop()
        {

            videoPlayerUI.GetVideoPlayer().SetVideoTime(slider.value);
            _isWait = true;
        }

        public override void OnKinelChangeVideoTime()
        {
            slider.value = videoPlayerUI.GetVideoPlayer().VideoTime;
        }

        public override void OnKinelVideoModeChange()
        {
            slider.value = 0;
        }
        
        public void FixedUpdate()
        {
            
            // if (Networking.GetServerTimeInMilliseconds() - updateTime <= updateInterval)
            //     return;
            
            if (!videoPlayerUI.GetVideoPlayer())
                return;

            if (!videoPlayerUI.GetVideoPlayer().IsPlaying)
                return;
            
            if (!videoPlayerUI.IsVideo())
                return;
            
            if (_isWait)
            {
                _waitTiem += Time.deltaTime;
                if (_waitTiem >= WAIT)
                {
                    _isDrag = false;
                    _isWait = false;
                    _waitTiem = 0;
                }
                slider.value = videoPlayerUI.GetSystem().GetTime();
                return;
            }

            if (!_isDrag)
                slider.value = videoPlayerUI.GetSystem().GetTime();
            
            // updateTime = Networking.GetServerTimeInMilliseconds();
            
        }

        public void SetSliderLength(float time)
        {
            this.slider.maxValue = time;
        }

        public bool IsDrag()
        {
            return _isDrag;
        }

        public void Lock()
        {
#if !UNITY_EDITOR
            if (Networking.LocalPlayer.isMaster)
                return;
#endif
            slider.interactable = false;
            slider.value = slider.maxValue;
        }

        public void Unlock()
        {
#if !UNITY_EDITOR
            if (Networking.LocalPlayer.isMaster)
                return;
#endif
            slider.interactable = true;
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


        public Slider GetSeekSlider()
        {
            return slider;
        }

        
        
        
    }
}