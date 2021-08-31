
using System;
using UdonSharp;

using UnityEngine;
using VRC.SDK3.Components.Video;
using VRC.SDK3.Video.Components.Base;
using VRC.SDKBase;
using VRC.Udon;

namespace Kinel.VideoPlayer.Scripts._2._0._0
{
    
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class KinelVideoPlayer : UdonSharpBehaviour
    {

        // Const variables
        public const string DEBUG_PREFIX = "[KineL]";
        
        // SerializeField variables
        [SerializeField] private KinelVideoPlayerController videoPlayerController;
        [SerializeField] private float deleyLimit;

        // System
        private UdonSharpBehaviour[] _listeners;

        // Synced variables
        [UdonSynced] private VRCUrl _syncedURL;
        [UdonSynced] private float _videoStartGlobalTime = 0;
        [UdonSynced] private float _pausedTime = 0;
        [UdonSynced] private int _globalVideoId = 0;
        [UdonSynced] private bool _isPlaying = false;
        [UdonSynced] private bool _isPause = false;
        
        
        // Local variables
        private float _videoStartLocalTime = 0;
        private float _lastSyncTime = 0;
        private int _localVideoId = 0;
        private bool _isPauseLocal = false;
        
        //public 
        
        public void Start()
        {

        }

        public void OnDisable()
        {
            
        }

        public void RegisterListener(UdonSharpBehaviour listener)
        {
            if(_listeners == null)
                _listeners = new UdonSharpBehaviour[0];
            
            var temp = new UdonSharpBehaviour[_listeners.Length + 1];
            for (int i = 0; i < _listeners.Length; i++)
            {
                temp[i] = _listeners[i];
            }

            temp[_listeners.Length] = listener;
            _listeners = temp;
            
            Debug.Log($"{DEBUG_PREFIX}" + " Register " + $"{listener.name}");
        }

        public void UnregisterListener(UdonSharpBehaviour listener)
        {
            var temp = new UdonSharpBehaviour[_listeners.Length];
            int i = 0;
            for (; i < _listeners.Length; i++)
            {
                temp[i] = _listeners[i];
                if (_listeners[i].name.Equals(listener.name))
                    break;
            }
            
            for (; i < _listeners.Length - 1; i++)
            {
                temp[i] = _listeners[i + 1];
            }

            temp[_listeners.Length - 1] = listener;
            _listeners = temp;
            
            Debug.Log($"{DEBUG_PREFIX}" + " Unregister " + $"{listener.name}");
        }

        public void CallEvent(string eventName)
        {
            foreach (UdonSharpBehaviour listener in _listeners)
            {
                Debug.Log($"Call listener {listener.name}, event {eventName}");
                listener.SendCustomEvent(eventName);
            }
            return;
        }

        public bool PlayByURL(VRCUrl url, bool ownerTransfer)
        {
            
            if(ownerTransfer)
                Networking.SetOwner(Networking.LocalPlayer, this.gameObject);

            _syncedURL = url;
            _globalVideoId++;
            _localVideoId = _globalVideoId;
            
            videoPlayerController.GetCurrentVideoPlayer().LoadURL(_syncedURL);
            
            this.RequestSerialization();
            _listeners[0].SendCustomEvent("OnVideoStart");

            return false;
        }

        public void Play()
        {
            _isPauseLocal = false;
            if (Networking.IsOwner(Networking.LocalPlayer, this.gameObject))
            {
                _isPlaying = true;
                _isPause = false;
                _videoStartGlobalTime += (float) Networking.GetServerTimeInSeconds() - _pausedTime;
                _pausedTime = 0;
                RequestSerialization();
            }
            videoPlayerController.GetCurrentVideoPlayer().Play();
        }

        public void Pause()
        {
            _isPauseLocal = true;
            if (Networking.IsOwner(Networking.LocalPlayer, this.gameObject))
            {
                _pausedTime = (float) Networking.GetServerTimeInSeconds();
                _isPlaying = false;
                _isPause = true;
                RequestSerialization();
            }
            videoPlayerController.GetCurrentVideoPlayer().Pause();
        }

        public void Reset()
        {
            if (Networking.IsOwner(Networking.LocalPlayer, this.gameObject))
            {
                _pausedTime = 0;
                _isPlaying = false;
                _isPause = false;
                _videoStartGlobalTime = 0;
                RequestSerialization();
            }
            _isPauseLocal = false;
            SendCustomEvent(nameof(ResetGlobal));
        }

        public void ResetGlobal()
        {
            _isPauseLocal = false;
            CallEvent("OnKinelVideoReset");
        }

        public void Sync()
        {
            if (_localVideoId == _globalVideoId && _isPlaying && !videoPlayerController.GetCurrentVideoPlayer().IsPlaying)
            {
                PlayByURL(_syncedURL, false);
            }
            _lastSyncTime = Time.realtimeSinceStartup;
            float globalVideoTime = Mathf.Clamp((float) Networking.GetServerTimeInSeconds() - _videoStartGlobalTime, 0, videoPlayerController.GetCurrentVideoPlayer().GetDuration());
            if (Mathf.Clamp(Mathf.Abs(videoPlayerController.GetCurrentVideoPlayer().GetTime() - globalVideoTime), 0, videoPlayerController.GetCurrentVideoPlayer().GetDuration()) > deleyLimit)
                videoPlayerController.GetCurrentVideoPlayer().SetTime(globalVideoTime);
        }

        public override void OnVideoReady()
        {
            CallEvent("OnKinelVideoReady");
            if (Networking.IsOwner(Networking.LocalPlayer, this.gameObject))
            {
                videoPlayerController.GetCurrentVideoPlayer().Play();
                return;
            }
            
            if (_isPlaying && !videoPlayerController.GetCurrentVideoPlayer().IsPlaying)
                videoPlayerController.GetCurrentVideoPlayer().Play();

            if (_isPause)
                Sync();
           
        }

        public override void OnVideoStart()
        {
            if (Networking.IsOwner(Networking.LocalPlayer, this.gameObject))
            {
                _videoStartGlobalTime = (float) Networking.GetServerTimeInSeconds();
                RequestSerialization();
            }
           
            CallEvent("OnKinelVideoStart");
        }

        public override void OnVideoEnd()
        {
            if (Networking.IsOwner(Networking.LocalPlayer, this.gameObject))
            {
                _videoStartGlobalTime = (float) Networking.GetServerTimeInSeconds();
                RequestSerialization();
            }
            CallEvent("OnKinelVideoEnd");
        }

        public override void OnVideoLoop()
        {
            if (Networking.IsOwner(Networking.LocalPlayer, this.gameObject))
            {
                _videoStartGlobalTime = (float) Networking.GetServerTimeInSeconds();
                RequestSerialization();
            }
            CallEvent("OnKinelVideoLoop");
        }

        public override void OnVideoError(VideoError videoError)
        {
            Debug.Log($"{videoError}");
            CallEvent(nameof(OnVideoError));
        }

        public override void OnDeserialization()
        {
            if (Networking.IsOwner(Networking.LocalPlayer, this.gameObject))
                return;

            if (_isPause != _isPauseLocal && _isPlaying)
            {
                if(_isPause)
                    Play();
                else 
                    Pause();
            }

            if (!videoPlayerController.GetCurrentVideoPlayer().IsPlaying && _isPlaying)
            {
                Debug.Log("Deseri Play");
                Play();
                return;
            }

            if (_localVideoId != _globalVideoId)
            {
                PlayByURL(_syncedURL, false);
                return;
            }
            
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (Networking.LocalPlayer != player)
                return;

            if (_isPlaying || (!_isPlaying && _isPause))
            {
                PlayByURL(_syncedURL, false);
                return;
            }
            
        }

        public void SetVideoTime(float seconds)
        {
            Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
            var video = videoPlayerController.GetCurrentVideoPlayer();
            _videoStartGlobalTime += video.GetTime() - seconds;
            video.SetTime(Mathf.Clamp((float) Networking.GetServerTimeInSeconds() - _videoStartGlobalTime, 0, video.GetDuration()));
            _videoStartGlobalTime += 1f;
            RequestSerialization();
        }
        
        public bool IsPlaying()
        {
            return _isPlaying;
        }

        public bool IsPause()
        {
            return _isPause;
        }

        public KinelVideoPlayerController GetVideoPlayerController()
        {
            return videoPlayerController;
        }

        public VRCUrl GetUrl()
        {
            return _syncedURL;
        }
        
        
        
        
    }
}

