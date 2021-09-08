
using System;
using UdonSharp;

using UnityEngine;
using VRC.SDK3.Components.Video;
using VRC.SDK3.Video.Components.Base;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace Kinel.VideoPlayer.Scripts._2._0._0
{
    
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class KinelVideoPlayer : UdonSharpBehaviour
    {

        // Const variables
        public const string DEBUG_PREFIX = "[<color=#58ACFA>KineL</color>]";
        
        // SerializeField variables
        [SerializeField] private KinelVideoPlayerController videoPlayerController;
        [SerializeField] private float deleyLimit;
        [SerializeField] private int retryLimit;
        [SerializeField] private bool enableErrorRetry;

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
        private int _errorCount = 0;
        private int _retryCount = 0;
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
                listener.SendCustomEvent(eventName);
            }
            return;
        }

        public bool PlayByURL(VRCUrl url, bool ownerTransfer)
        {
            
            if(ownerTransfer)
                Networking.SetOwner(Networking.LocalPlayer, this.gameObject);

            if (Networking.IsOwner(Networking.LocalPlayer, this.gameObject))
            {
                _syncedURL = url;
                _globalVideoId++;
                RequestSerialization();
            }
            _localVideoId = _globalVideoId;
            
            videoPlayerController.GetCurrentVideoPlayer().LoadURL(_syncedURL);
            
            return true;
        }

        public void Play()
        {
            Debug.Log($"{DEBUG_PREFIX} Video playing...");
            _isPauseLocal = false;
            if (Networking.IsOwner(Networking.LocalPlayer, this.gameObject))
            {
                _isPlaying = true;
                _isPause = false;
                _videoStartGlobalTime += (float) Networking.GetServerTimeInSeconds() - _pausedTime;
                _pausedTime = 0;
                RequestSerialization();
            }

            _videoStartLocalTime = _videoStartGlobalTime;
            videoPlayerController.GetCurrentVideoPlayer().Play();
        }

        public void Pause(bool ownerTransfer)
        {

            if (!_isPlaying)
                return;
            
            if(ownerTransfer)
                Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
            
            _isPauseLocal = true;
            if (Networking.IsOwner(Networking.LocalPlayer, this.gameObject))
            {
                Debug.Log("Pause Owner");
                _pausedTime = (float) Networking.GetServerTimeInSeconds();
                _isPlaying = false;
                _isPause = true;
                RequestSerialization();
            }
            videoPlayerController.GetCurrentVideoPlayer().Pause();
        }

        public void ResetGlobal()
        {
            Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
            _pausedTime = 0;
            _isPlaying = false;
            _isPause = false;
            _videoStartGlobalTime = 0;
            _syncedURL = VRCUrl.Empty;
            RequestSerialization();
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(Reset));
        }

        public void Reset()
        {
            _isPauseLocal = false;
            _videoStartLocalTime = 0;
            videoPlayerController.GetCurrentVideoPlayer().Stop();
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

            if (Networking.IsOwner(Networking.LocalPlayer, this.gameObject))
            {
                Play();
                CallEvent("OnKinelVideoReady");
                return;
            }
            
            if (_isPlaying && !videoPlayerController.GetCurrentVideoPlayer().IsPlaying)
                videoPlayerController.GetCurrentVideoPlayer().Play();

            if (_isPause)
                Sync();
            
            CallEvent("OnKinelVideoReady");
           
        }

        public override void OnVideoStart()
        {
            
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

        private float _lastVideoErrorTime = 0;

        public override void OnVideoError(VideoError videoError)
        {
            // 連続エラー防止
            float nowTime = Networking.GetServerTimeInMilliseconds();
            Debug.Log($"{DEBUG_PREFIX} Video error. : {videoError}");
            
            Reset();
            
            //Debug.Log($"{DEBUG_PREFIX} Video error. : {videoError}");
            //CallEvent("OnKinelVideoRetryError");
            
            _lastVideoErrorTime = nowTime;
            _errorCount++;
            
            if (enableErrorRetry)
            {
                if (_retryCount > retryLimit)
                {
                    Debug.Log($"{DEBUG_PREFIX} Sorry. Couldn't retry.");
                    Reset();
                    _retryCount = 0;
                    _errorCount = 0;
                    CallEvent("OnKinelVideoRetryError");
                    return;
                }
                Debug.Log($"{_retryCount}, {_errorCount}");
                _retryCount++;
                Debug.Log($"{DEBUG_PREFIX} Retrying...  Please wait.");
                SendCustomEventDelayedSeconds(nameof(Reload), 5f);
            }
            
            CallEvent("OnKinelVideoError");
        }

        public void ReloadGlobal()
        {
            Debug.Log($"{DEBUG_PREFIX} Reloading...");
            var lastVideoURL = _syncedURL;
            ResetGlobal();
            PlayByURL(lastVideoURL, true);
            Debug.Log($"{DEBUG_PREFIX} Reload completed. # {_syncedURL}");
        }

        public void Reload()
        {
            Debug.Log($"{DEBUG_PREFIX} Reloading...");
            Reset();
            videoPlayerController.GetCurrentVideoPlayer().LoadURL(_syncedURL);
            Debug.Log($"{DEBUG_PREFIX} Reload completed. # {_syncedURL}");
        }

        public override void OnDeserialization()
        {
            if (Networking.IsOwner(Networking.LocalPlayer, this.gameObject))
                return;

            if (_isPause != _isPauseLocal)
            {
                Debug.Log($"{DEBUG_PREFIX} Deserialization: paused.");
                if(_isPause)
                    Pause(false);
                else 
                    Play();
                return;
            }

            if (!videoPlayerController.GetCurrentVideoPlayer().IsPlaying && _isPlaying)
            {
                Debug.Log($"{DEBUG_PREFIX} Deserialization: Video start.");
                Play();
                return;
            }
            
            if (_videoStartLocalTime != _videoStartGlobalTime)
                Sync();

            if (_localVideoId != _globalVideoId)
            {
                Debug.Log($"{DEBUG_PREFIX} Deserialization: URL synced.");
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
                Debug.Log($"{DEBUG_PREFIX} Player Joined. now playing video...");
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

