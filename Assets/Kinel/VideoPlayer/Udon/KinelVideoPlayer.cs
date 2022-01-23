using Kinel.VideoPlayer.Udon.Controller;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components.Video;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

namespace Kinel.VideoPlayer.Udon
{

    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class KinelVideoPlayer : UdonSharpBehaviour
    {

        // Const variables
        public const string DEBUG_PREFIX = "[<color=#58ACFA>KineL</color>]";
        public const int VIDEO_MODE = 0;
        public const int STREAN_MODE = 1;

        // SerializeField variables
        [SerializeField] private KinelVideoPlayerController videoPlayerController;
        [SerializeField] private float deleyLimit;
        [SerializeField] private int retryLimit;
        [SerializeField] private bool enableErrorRetry;
        [SerializeField] private bool autoPlay;
        [SerializeField] private bool loop;

        // System
        private UdonSharpBehaviour[] _listeners;

        // Synced variables
        [UdonSynced, FieldChangeCallback(nameof(SyncedUrl))]
        private VRCUrl _syncedUrl;
        
        [UdonSynced, FieldChangeCallback(nameof(VideoStartGlobalTime))]
        private float _videoStartGlobalTime = 0;
        [UdonSynced] private float _pausedTime = 0;
        
        [UdonSynced, FieldChangeCallback(nameof(IsPlaying))] 
        private bool _isPlaying = false;
        
        [UdonSynced, FieldChangeCallback(nameof(IsPause))] 
        private bool _isPause = false;

        [UdonSynced, FieldChangeCallback(nameof(IsLock))]
        private bool _isLock = false;

        // Local variables
        private float _videoStartLocalTime = 0;
        private float _lastSyncTime = 0;
        private int _localVideoId = 0;
        private int _errorCount = 0;
        private int _retryCount = 0;
        private bool _isPauseLocal = false;


        public void Start()
        {

        }

        public void OnDisable()
        {

        }

        public VRCUrl SyncedUrl
        {
            get
            {
                return _syncedUrl;
            }
            set
            {
                _syncedUrl = value;
                Debug.Log($"{DEBUG_PREFIX} Deserialization: URL synced.");
                PlayByURL(_syncedUrl);
                return;
            }
        }

        public float VideoStartGlobalTime
        {
            get => _videoStartGlobalTime;
            set
            {
                _videoStartGlobalTime = value;
                Sync();
            }
        }

        public bool IsPlaying
        {
            get
            {
                return _isPlaying;
            }
            set
            {
                _isPlaying = value;
                if (!videoPlayerController.GetCurrentVideoPlayer().IsPlaying && _isPlaying)
                {
                    Debug.Log($"{DEBUG_PREFIX} Deserialization: Video start.");
                    Play();
                }
            }
        }

        public bool IsPause
        {
            get => _isPause; 
            set
            {
                Debug.Log($"{DEBUG_PREFIX} Deserialization: paused.");
                _isPause = value;
                if (_isPause)
                    Pause();
                else
                    Play();

            }
        }

        public bool IsLock
        {
            get => _isLock;
            set
            {
                _isLock = value;
                CallEvent("OnKinelVideoPlayerLock");
            }
        }

        public void RegisterListener(UdonSharpBehaviour listener)
        {
            if (_listeners == null)
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

        public bool PlayByURL(VRCUrl url)
        {

            Debug.Log($"{DEBUG_PREFIX} Video load process starting...");

            if (Networking.IsOwner(Networking.LocalPlayer, this.gameObject))
            {
                _syncedUrl = url;
                //_globalVideoId++;
                RequestSerialization();
            }

           //_localVideoId = _globalVideoId;
            
            CallEvent("OnUrlUpdate");

            videoPlayerController.GetCurrentVideoPlayer().LoadURL(_syncedUrl);

            return true;
        }

        public void Play()
        {
            Debug.Log($"{DEBUG_PREFIX} Video playing...");
            _isPauseLocal = false;
            if (Networking.IsOwner(Networking.LocalPlayer, this.gameObject))
            {
                if (!IsPlaying && !IsPause)
                    _videoStartGlobalTime = (float) Networking.GetServerTimeInSeconds();
                else 
                    _videoStartGlobalTime += (float) Networking.GetServerTimeInSeconds() - _pausedTime;
                
                _isPlaying = true;
                _isPause = false;
                
                    
                _pausedTime = 0;
                RequestSerialization();
            }

            _videoStartLocalTime = _videoStartGlobalTime;
            videoPlayerController.GetCurrentVideoPlayer().Play();
        }

        public void Pause()
        {

            if (!_isPlaying)
                return;

            if (Networking.IsOwner(Networking.LocalPlayer, this.gameObject))
            {
                Debug.Log("Pause Owner");
                _pausedTime = (float)Networking.GetServerTimeInSeconds();
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
            _syncedUrl = VRCUrl.Empty;
            RequestSerialization();
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(Reset));
        }

        public void Reset()
        {
            if (videoPlayerController.GetCurrentVideoMode() == null) return;
            
            _isPauseLocal = false;
            _videoStartLocalTime = 0;
            videoPlayerController.GetCurrentVideoPlayer().Stop();
            CallEvent("OnKinelVideoReset");
        }

        public void Sync()
        {
            if (/*_localVideoId == _globalVideoId && */_isPlaying &&
                !videoPlayerController.GetCurrentVideoPlayer().IsPlaying)
            {
                PlayByURL(_syncedUrl);
            }

            _lastSyncTime = Time.realtimeSinceStartup;
            float globalVideoTime = Mathf.Clamp((float)Networking.GetServerTimeInSeconds() - _videoStartGlobalTime, 0,
                videoPlayerController.GetCurrentVideoPlayer().GetDuration());
            if (Mathf.Clamp(Mathf.Abs(videoPlayerController.GetCurrentVideoPlayer().GetTime() - globalVideoTime), 0,
                videoPlayerController.GetCurrentVideoPlayer().GetDuration()) > deleyLimit)
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
            Sync();
            CallEvent("OnKinelVideoStart");
        }

        // Loop Onの時は呼ばれない。 LoopがOFFの時だけ呼ばれる
        public override void OnVideoEnd()
        {
            
            Debug.Log("OnVideoEnd");
            
            if (Networking.IsOwner(Networking.LocalPlayer, this.gameObject))
            {
                _videoStartGlobalTime = (float) Networking.GetServerTimeInSeconds();
                _isPause = false;
                _isPlaying = false;
                RequestSerialization();
            }

            CallEvent("OnKinelVideoEnd");
        }

        public override void OnVideoLoop()
        {
            Debug.Log("OnVideoLoop");
            if (Networking.IsOwner(Networking.LocalPlayer, this.gameObject))
            {
                _videoStartGlobalTime = (float)Networking.GetServerTimeInSeconds();
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
                    ResetRetryProcess();
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

        public void ResetRetryProcess()
        {
            Reset();
            _retryCount = 0;
            _errorCount = 0;
        }

        public void ReloadGlobal()
        {
            Debug.Log($"{DEBUG_PREFIX} Reloading...");
            var lastVideoURL = _syncedUrl;
            ResetGlobal();
            TakeOwnership();
            PlayByURL(lastVideoURL);
            Debug.Log($"{DEBUG_PREFIX} Reload completed. # {_syncedUrl}");
        }

        public void Reload()
        {
            Debug.Log($"{DEBUG_PREFIX} Reloading...");
            Reset();
            videoPlayerController.GetCurrentVideoPlayer().LoadURL(_syncedUrl);
            Debug.Log($"{DEBUG_PREFIX} Reload internal process start. # {_syncedUrl}");
        }

        // public override void OnDeserialization()
        // {
        //     if (Networking.IsOwner(Networking.LocalPlayer, this.gameObject))
        //         return;
        //
        //     // if (!videoPlayerController.GetCurrentVideoPlayer().IsPlaying && _isPlaying)
        //     // {
        //     //     Debug.Log($"{DEBUG_PREFIX} Deserialization: Video start.");
        //     //     Play();
        //     //     return;
        //     // }
        //
        //     if (_videoStartLocalTime != _videoStartGlobalTime)
        //         Sync();
        //
        // }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (Networking.LocalPlayer != player)
                return;

            if (_isPlaying || (!_isPlaying && _isPause))
            {
                Debug.Log($"{DEBUG_PREFIX} Player Joined. now playing video...");
                PlayByURL(_syncedUrl);
                return;
            }

        }

        public void SetVideoTime(float seconds)
        {
            TakeOwnership();
            var video = videoPlayerController.GetCurrentVideoPlayer();
            _videoStartGlobalTime += video.GetTime() - seconds;
            video.SetTime(Mathf.Clamp((float) Networking.GetServerTimeInSeconds() - _videoStartGlobalTime, 0,
                video.GetDuration()));
            _videoStartGlobalTime += 1f;
            RequestSerialization();
            CallEvent("OnChangeVideoTime");
        }

        public void SetLoop(bool loop)
        {
            videoPlayerController.Loop(loop);
        }

        public bool isLoop()
        {
            return loop;
        }

        public void ChangeMode(int mode)
        {
            videoPlayerController.ChangeMode(mode);
        }
        
        public KinelVideoPlayerController GetVideoPlayerController()
        {
            return videoPlayerController;
        }

        public int GetCurrentVideoMode()
        {
            return videoPlayerController.GetCurrentVideoMode();
        }

        public VRCUrl GetUrl()
        {
            return _syncedUrl;
        }

        public int GetErrorRetryCount()
        {
            return _retryCount;
        }

        public bool IsErrorRetry()
        {
            return enableErrorRetry;
        }

        public void TakeOwnership()
        {
            if (Networking.IsOwner(gameObject))
                return;

            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }
        
        
        
        
    }
}

