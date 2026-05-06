using System;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Components.Video;
using VRC.SDK3.Video.Components.AVPro;
using VRC.SDK3.Video.Interfaces.AVPro;
using VRC.SDKBase;
#if KINEL_AVPRO_VIDEO_ENABLED  
using RenderHeads.Media.AVProVideo;
#endif

namespace Kinel.VideoPlayer.V3.Scripts
{
#if KINEL_AVPRO_VIDEO_ENABLED         
    [DefaultExecutionOrder(1)]
    public class KinelAvProVideoResolver: IAVProVideoPlayerInternal
    {
        protected static readonly string DebugLogPrefix = "[<color=#58ACFA>KineL</color><color=#ffff00>#AVPRO</color>]";

        public static System.Action<VRCUrl, int, UnityEngine.Object, Action<string>, Action<VideoError>> StartResolveURLCoroutine { get; set; }
        
        public KinelAvProVideoResolver(VRCAVProVideoPlayer videoPlayer, MediaPlayer mediaPlayer)
        {
            _mediaPlayer = mediaPlayer;
            _mediaPlayer.Events.AddListener(OnMediaPlayerEvent);
            
            _videoPlayer = videoPlayer;
            Loop = videoPlayer.Loop;
            UseLowLatency = videoPlayer.UseLowLatency;
            VideoWidth = videoPlayer.VideoWidth;
            VideoHeight = videoPlayer.VideoHeight;
        }
        

        private MediaPlayer _mediaPlayer;
        private VRCAVProVideoPlayer _videoPlayer;
        public bool Loop { get; set; }
        public bool IsPlaying => _mediaPlayer.Control.IsPlaying();
        public bool IsReady { get; }
        public bool UseLowLatency { get; }
        public int VideoWidth { get; }
        public int VideoHeight { get; }
        
        [SerializeField]
        private int maximumResolution = 720;

        public void LoadURL(VRCUrl url)
        {
            if (KinelAvProVideoResolver.StartResolveURLCoroutine != null)
            {
                KinelAvProVideoResolver.StartResolveURLCoroutine(url, this.maximumResolution, (UnityEngine.Object) _videoPlayer, new Action<string>(PlayVideo),  _videoPlayer.OnVideoError);
            }
            else
                PlayVideo(url.Get());
            void PlayVideo(string resolvedURL)
            {
                this._mediaPlayer.OpenMedia(MediaPathType.AbsolutePathOrURL, resolvedURL, false);
                _videoPlayer.OnVideoReady();
            };
        }

        public void PlayURL(VRCUrl url)
        {
            if (KinelAvProVideoResolver.StartResolveURLCoroutine != null)
                KinelAvProVideoResolver.StartResolveURLCoroutine(url, this.maximumResolution, (UnityEngine.Object) _videoPlayer, new Action<string>(PlayVideo),  _videoPlayer.OnVideoError);
            else
                PlayVideo(url.Get());

            void PlayVideo(string resolvedURL)
            {
                this._mediaPlayer.OpenMedia(MediaPathType.AbsolutePathOrURL, resolvedURL, true);
                _videoPlayer.OnVideoStart();
            };
        }

        public void Play()
        {
            _mediaPlayer.Control.Play();
            _videoPlayer.OnVideoStart();
        }

        public void Pause()
        {
            _mediaPlayer.Control.Pause();
        }

        public void Stop()
        {
            _mediaPlayer.Control.Stop();
            if (!Loop)
            {
                _videoPlayer.OnVideoEnd();
            }
        }

        public void SetTime(float value)
        {
            _mediaPlayer.Control.Seek(value);
        }

        public float GetTime()
        {
            return _mediaPlayer != null ? (float)_mediaPlayer.Control.GetCurrentTime() : 0f;
        }

        public float GetDuration()
        {
            return _mediaPlayer != null ? (float)_mediaPlayer.Info.GetDuration() : 0f;
        }
        
        private void OnMediaPlayerEvent(MediaPlayer mediaPlayer, MediaPlayerEvent.EventType eventType, ErrorCode errorCode)
        {
            switch (eventType)
            {
                case MediaPlayerEvent.EventType.FinishedPlaying:
                    if (Loop)
                    {
                        mediaPlayer.Control.Rewind();
                        mediaPlayer.Control.Play();
                        _videoPlayer.OnVideoLoop();
                    }
                    break;
            }
        }
    }
#endif
}