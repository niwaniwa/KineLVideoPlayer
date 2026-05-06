using System;
using HoshinoLabs.IwaSync3.Udon;
using Kinel.VideoPlayer.V3.Udon.System.Component;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components.Video;

namespace Kinel.VideoPlayer.V3.Udon.System.Addon
{
    /// <summary>
    /// 通常であればKinelMediaBaseがイベントを受け取るが、今回はiwasyncが中間にあって直接データを取れないのでBridgeする
    /// </summary>
    public class KinelIwasyncBridge : VideoCoreEventListener
    {
        [SerializeField] private IwasyncProxy iwasyncProxy;

        public void Start()
        {
            InitIwasyncComponents();
        }

        private void InitIwasyncComponents()
        {
        }

        public override void OnVideoEnd()
        {
            iwasyncProxy.OnVideoEnd();
        }

        public override void OnVideoError(VideoError videoError)
        {
            iwasyncProxy.OnVideoError(videoError);
        }

        public override void OnVideoLoop()
        {
            iwasyncProxy.OnVideoLoop();
        }

        public override void OnVideoReady()
        {
            iwasyncProxy.OnVideoReady();
        }

        public override void OnVideoStart()
        {
            iwasyncProxy.OnVideoStart();
        }


        public override void OnPlayerPlay()
        {
            iwasyncProxy.OnVideoPlay();
        }

        public override void OnPlayerPause()
        {
            iwasyncProxy.OnVideoPause();
        }

        public override void OnPlayerStop()
        {
            iwasyncProxy.OnVideoEnd();
        }

        public override void OnChangeURL()
        {
            iwasyncProxy.VideoKinelVideoListener.OnKinelLoadUrl(iwasyncProxy.GetPlayingUrl());
        }

        public override void OnChangeLoop()
        {
            // iwasyncProxy.VideoKinelVideoListener.OnCh
        }

        public override void OnChangeLive()
        {
            iwasyncProxy.VideoKinelVideoListener.OnKinelMediaTypeChanged();
        }

        public override void OnChangeSpeed()
        {
            // iwasyncProxy.VideoKinelVideoListener.OnS();
        }

        public override void OnChangeMaximumResolution()
        {
            // iwasyncProxy.VideoKinelVideoListener.OnKinelMediaTypeChanged();
        }

        public override void OnChangeMessage()
        {
            // iwasyncProxy.VideoKinelVideoListener.OnKinelMediaTypeChanged();
        }

        public override void OnChangeLock()
        {
            // iwasyncProxy.VideoKinelVideoListener.OnKinelLockChanged();
        }

        public override void OnChangeMute()
        {
            // iwasyncProxy.VideoKinelVideoListener.OnKinelMuteChanged();
        }

        public override void OnChangeVolume()
        {
            // iwasyncProxy.VideoKinelVideoListener.OnKinelVolumeChanged();
        }
    }
}