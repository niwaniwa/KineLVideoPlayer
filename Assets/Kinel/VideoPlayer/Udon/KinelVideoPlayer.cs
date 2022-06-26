using Kinel.VideoPlayer.Udon.Controller;
using Kinel.VideoPlayer.Udon.Module;
using UdonSharp;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRC.SDK3.Components.Video;
using VRC.SDK3.Video.Components.Base;
using VRC.SDKBase;
using VRC.Udon.Common.Enums;
using VRC.Udon.Common.Interfaces;

namespace Kinel.VideoPlayer.Udon
{

    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class KinelVideoPlayer : UdonSharpBehaviour
    {

        // Const variables
        public const string DEBUG_PREFIX = "[<color=#58ACFA>KineL</color>]";
        public const int VIDEO_MODE = 0;
        public const int STREAM_MODE = 1;

        // SerializeField variables
        [SerializeField] private KinelVideoPlayerController videoPlayerController;
        [SerializeField] private float deleyLimit;
        [SerializeField] private int retryLimit;
        [SerializeField] private bool enableErrorRetry;
        [SerializeField] private bool defaultLoop;
        [SerializeField] private bool enableDefaultUrl;
        [SerializeField] private VRCUrl defaultPlayUrl;
        [SerializeField] private int defaultPlayUrlMode;

        // System
        private UdonSharpBehaviour[] _listeners;

        // Synced variables
        [UdonSynced, FieldChangeCallback(nameof(SyncedUrl))]
        private VRCUrl _syncedUrl;
        
        [UdonSynced, FieldChangeCallback(nameof(VideoStartGlobalTime))]
        private float _videoStartGlobalTime = 0;
        [UdonSynced, FieldChangeCallback(nameof(PausedTime))] private float _pausedTime = 0;
        
        [UdonSynced, FieldChangeCallback(nameof(IsPlaying))] 
        private bool _isPlaying = false;
        
        [UdonSynced, FieldChangeCallback(nameof(IsPause))] 
        private bool _isPause = false;

        [UdonSynced, FieldChangeCallback(nameof(IsLock))]
        private bool _isLock = false;
        
        [UdonSynced, FieldChangeCallback(nameof(Loop))]
        private bool _loop;

        // Local variables
        private float _lastSyncTime = 0;
        private int _errorCount = 0;
        private int _retryCount = 0;
        private bool _loading = false;
        private bool _canceled = false;
        private bool _isVideo = true;

        private KinelScreenModule[] _screenModules;

        public void Start()
        {
            if(Networking.IsOwner(Networking.LocalPlayer, gameObject))
                Loop = defaultLoop;
        }

        public void OnDisable()
        {

        }

        public VRCUrl SyncedUrl
        {
            get => _syncedUrl;
            set
            {
                _syncedUrl = value;
                if (_syncedUrl.Equals(VRCUrl.Empty))
                    return;
                Debug.Log($"{DEBUG_PREFIX} Deserialization: URL synced.");
                
                
                PlayByURL(_syncedUrl);
                
            }
        }

        public float VideoStartGlobalTime
        {
            get => _videoStartGlobalTime;
            set
            {
                _videoStartGlobalTime = value;
                Debug.Log($"{DEBUG_PREFIX} Deserialization: Video Start Global Time ");
                SendCustomEventDelayedSeconds(nameof(Sync), 1);
            }
        }

        public bool IsPlaying
        {
            get => _isPlaying;
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
                
                CallEvent(_isLock ? "OnKinelVideoPlayerLocked" : "OnKinelVideoPlayerUnlocked");
            }
        }

        public bool Loop
        {
            get => _loop;
            set
            {
                _loop = value;
                SetLoop(_loop);
                CallEvent("OnKinelLoopChanged");
            }
        }

        /// <summary>
        /// 速度変更に依存しない現時点での動画再生時間を取得できます。
        /// 速度変更された動作再生時間についてはBaseVRCVideoPlayerから取得してください。
        /// </summary>
        public float VideoTime
        {
            get
            {
                float videoTime = Mathf.Clamp((IsPause ? _pausedTime : (float) Networking.GetServerTimeInSeconds()) - _videoStartGlobalTime,
                    0,
                    videoPlayerController.GetCurrentVideoPlayer().GetDuration());
                                return videoTime;
            }
            set => SetVideoTime(value);
        }

        public float PausedTime
        {
            get => _pausedTime;
            set => _pausedTime = value;
        }

        /// <summary>
        /// このビデオプレイヤーからイベントを受け取るためのリスナー登録関数
        /// </summary>
        /// <param name="listener">受け取るUdonSharpBehaviour</param>
        public void RegisterListener(UdonSharpBehaviour listener)
        {
            if (_listeners == null)
            {
                _listeners = new UdonSharpBehaviour[0];
                _screenModules = new KinelScreenModule[0];
            }

            var temp = new UdonSharpBehaviour[_listeners.Length + 1];
            var isScreen = (listener.name.Equals("KineLVP Screen")); //(listener.GetType() == typeof(KinelScreenModule));
            KinelScreenModule[] screenTemp = null;
            if (isScreen)
                screenTemp = new KinelScreenModule[_screenModules.Length + 1];
            
            for (int i = 0; i < _listeners.Length; i++)
            {
                temp[i] = _listeners[i];
            }

            if (isScreen)
            {
                for (int i = 0; i < _screenModules.Length; i++)
                {
                    screenTemp[i] = _screenModules[i];
                }
                screenTemp[_screenModules.Length] = (KinelScreenModule) listener;
                _screenModules = screenTemp;
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

        /// <summary>
        /// イベントの呼び出し
        /// </summary>
        /// <param name="eventName">イベント名</param>
        public void CallEvent(string eventName)
        {
            foreach (UdonSharpBehaviour listener in _listeners)
            {
                listener.SendCustomEvent(eventName);
            }

            return;
        }

        public void CallEventDelayedFrames(string eventName, int frames)
        {
            foreach (UdonSharpBehaviour listener in _listeners)
            {
                listener.SendCustomEventDelayedFrames(eventName, frames);
            }

            return;
        }

        /// <summary>
        /// 任意のVRCUrlの動画を再生します。
        /// </summary>
        /// <param name="url">再生する動画URL</param>
        public void PlayByURL(VRCUrl url)
        {

            Debug.Log($"{DEBUG_PREFIX} Video load process starting...");

            if (Networking.IsOwner(Networking.LocalPlayer, this.gameObject))
            {
                _syncedUrl = url;
                //_globalVideoId++;
                RequestSerialization();
            }

            //_localVideoId = _globalVideoId;
            
            CallEvent("OnKinelUrlUpdate");
            _loading = true;
            videoPlayerController.GetCurrentVideoPlayer().LoadURL(_syncedUrl);
        }

        /// <summary>
        /// 動画の再生を行います。
        /// 動画が一時停止の場合 - 続きから再生
        /// 動画が終了した場合   - 
        /// </summary>
        public void Play()
        {
            Debug.Log($"{DEBUG_PREFIX} Video playing...");

            if (_syncedUrl == null)
                return;
            
            if (Networking.IsOwner(Networking.LocalPlayer, this.gameObject))
            {
                
                if (!IsPause)
                    _videoStartGlobalTime = (float) Networking.GetServerTimeInSeconds();

                if(IsPause)
                    _videoStartGlobalTime += (float) Networking.GetServerTimeInSeconds() - _pausedTime;
                
                _isPlaying = true;
                _isPause = false;
                _pausedTime = 0;
                RequestSerialization();
            }


            videoPlayerController.GetCurrentVideoPlayer().Play();
        }

        public void Pause()
        {

            if (!_isPlaying)
                return;

            if (Networking.IsOwner(Networking.LocalPlayer, gameObject))
            {
                _pausedTime = (float) Networking.GetServerTimeInSeconds();
                _isPlaying = false;
                _isPause = true;
                RequestSerialization();
            }

            videoPlayerController.GetCurrentVideoPlayer().Pause();
            CallEvent("OnKinelVideoPause");
        }

        public void ResetGlobal()
        {
            Debug.Log($"{DEBUG_PREFIX} Reset (Global)");
            TakeOwnership();
            _pausedTime = 0;
            _isPlaying = false;
            _isPause = false;
            _videoStartGlobalTime = 0;
            _syncedUrl = VRCUrl.Empty;
            RequestSerialization();
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(ResetLocal));
        }

        public void ResetLocal()
        {
            Debug.Log($"{DEBUG_PREFIX} Reset (Local)");
            if (videoPlayerController.GetCurrentVideoPlayer() == null) return;
            videoPlayerController.GetUnityVideoPlayer().Stop();
            videoPlayerController.GetAvProVideoPlayer().Stop();
            CallEvent("OnKinelVideoReset");
        }

        public void Sync()
        {
            if (IsPlaying && !videoPlayerController.GetCurrentVideoPlayer().IsPlaying)
            {
                if (_loading)
                {
                    Debug.Log($"{DEBUG_PREFIX} Already reloading....");
                    return;
                }
                
                Debug.Log($"{DEBUG_PREFIX} Synced. Reloading....");
                Reload();
            }
            _lastSyncTime = Time.realtimeSinceStartup;

            if (Mathf.Clamp(Mathf.Abs(videoPlayerController.GetCurrentVideoPlayer().GetTime() - VideoTime), 0,
                    videoPlayerController.GetCurrentVideoPlayer().GetDuration()) > deleyLimit)
            {
                Debug.Log($"{DEBUG_PREFIX} Synced. Set VideoTime ");
                videoPlayerController.GetCurrentVideoPlayer().SetTime(VideoTime);
            }

            Debug.Log($"{DEBUG_PREFIX} Synced.");

        }
        
        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (Networking.LocalPlayer != player)
                return;

            if (!Networking.LocalPlayer.IsOwner(gameObject))
                return;
            
            if (enableDefaultUrl)
            {
                SendCustomEventDelayedSeconds(nameof(PlayDefaultUrl), 5f);
            }
        }

        public void PlayDefaultUrl()
        {
            if (IsPlaying)
                return;
            
            TakeOwnership();
            ChangeMode(defaultPlayUrlMode);
            PlayByURL(defaultPlayUrl);
        }
        

        public override void OnVideoReady()
        {
            if (videoPlayerController.GetCurrentVideoMode() == STREAM_MODE)
            {
                if (float.IsInfinity(videoPlayerController.GetCurrentVideoPlayer().GetDuration()))
                    _isVideo = false;
            }
            else
            {
                _isVideo = true;
            }

            _loading = false;
            
            if (Networking.IsOwner(Networking.LocalPlayer, this.gameObject))
            {
                videoPlayerController.GetCurrentVideoPlayer().SetTime(0);
                _isPause = false;
                Play();
                CallEvent("OnKinelVideoReady");
                return;
            }

            if (_isPlaying && !videoPlayerController.GetCurrentVideoPlayer().IsPlaying)
                videoPlayerController.GetCurrentVideoPlayer().Play();

            if (_isPause)
            {
                Debug.Log($"{DEBUG_PREFIX} Video Ready");
                Sync();
            }
                

            CallEvent("OnKinelVideoReady");

        }

        public override void OnVideoStart()
        {
            Debug.Log($"{DEBUG_PREFIX} Video Start");
            Sync();
            
            CallEvent("OnKinelVideoStart");
            
        }

        // Loop Onの時は呼ばれない。 LoopがOFFの時だけ呼ばれる
        public override void OnVideoEnd()
        {
            
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
            if (Networking.IsOwner(Networking.LocalPlayer, this.gameObject))
            {
                _videoStartGlobalTime = (float)Networking.GetServerTimeInSeconds();
                RequestSerialization();
            }

            CallEvent("OnKinelVideoLoop");
        }

        private VideoError lastVideoError = VideoError.Unknown;

        public VideoError LastVideoError
        {
            get => lastVideoError;
            private set => lastVideoError = value;
        }

        public override void OnVideoError(VideoError videoError)
        {

            lastVideoError = videoError;
            Debug.Log($"{DEBUG_PREFIX} Video error. : {videoError}");
            
            _errorCount++;

            if (enableErrorRetry)
            {
                Debug.Log($"{DEBUG_PREFIX} enableErrorRetry");
                if (_canceled)
                {
                    Debug.Log($"{DEBUG_PREFIX} retry canceled.");
                    ResetLocal();
                    ResetRetryProcess();
                    _canceled = false;
                    CallEvent("OnKinelVideoRetryError");
                    return;
                }
                
                if (_retryCount > retryLimit)
                {
                    Debug.Log($"{DEBUG_PREFIX} Sorry. Couldn't retry.");
                    ResetLocal();
                    ResetRetryProcess();
                    CallEvent("OnKinelVideoRetryError");
                    return;
                }
                
    
                Debug.Log($"{_retryCount}, {_errorCount}");
                _retryCount++;
                Debug.Log($"{DEBUG_PREFIX} Retrying...  Please wait.");
                SendCustomEventDelayedSeconds(nameof(Reload), 5f);
            }
            Debug.Log($"{DEBUG_PREFIX} CALL Event");
            CallEvent("OnKinelVideoError");
        }

        public void ResetRetryProcess()
        {
            _retryCount = 0;
            _errorCount = 0;
            _canceled = true;
        }

        public void ReloadGlobal()
        {
            if (_syncedUrl.Equals(VRCUrl.Empty))
            {
                Debug.Log($"{DEBUG_PREFIX} url is empty.");
                return;
            }
            Debug.Log($"{DEBUG_PREFIX} Reloading... (Global)");
            var lastVideoURL = _syncedUrl;
            ResetGlobal();
            TakeOwnership();
            PlayByURL(lastVideoURL);
            Debug.Log($"{DEBUG_PREFIX} Reload completed. # {_syncedUrl}");
        }

        private float lastReloadTime = -1;
        
        public void Reload()
        {
            
            if ((float) Networking.GetServerTimeInSeconds() - lastReloadTime <= 4 && !(lastReloadTime < 0) || _syncedUrl.Equals(VRCUrl.Empty))
            {
                Debug.Log($"{DEBUG_PREFIX} Reload error: {(float) Networking.GetServerTimeInSeconds() - lastReloadTime}");
                return;
            }

            lastReloadTime = (float) Networking.GetServerTimeInSeconds();
            Debug.Log($"{DEBUG_PREFIX} Reloading... (Local)");
            ResetLocal();
            videoPlayerController.GetCurrentVideoPlayer().LoadURL(_syncedUrl);
            Debug.Log($"{DEBUG_PREFIX} Reload internal process start. # {_syncedUrl}");
        }

        public void SetVideoTime(float seconds)
        {
            Debug.Log($"{DEBUG_PREFIX} Set time. {seconds}");
            TakeOwnership();
            var video = videoPlayerController.GetCurrentVideoPlayer();
            _videoStartGlobalTime += VideoTime - seconds;
            video.SetTime(Mathf.Clamp((float) Networking.GetServerTimeInSeconds() - _videoStartGlobalTime, 0,
                video.GetDuration()));

            RequestSerialization();
            if (_isPause)
            {
                CallEventDelayedFrames("OnKinelChangeVideoTime", 20);
                return;
            }
            CallEvent("OnKinelChangeVideoTime");
        }

        public void SetLoop(bool loop)
        {
            videoPlayerController.Loop(loop);
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
            return videoPlayerController.CurrentMode;
        }

        public VRCUrl GetUrl()
        {
            return _syncedUrl;
        }

        public int GetErrorRetryCount()
        {
            return _retryCount;
        }

        public KinelScreenModule[] GetKinelScreenModules()
        {
            return _screenModules;
        }

        public bool IsErrorRetry()
        {
            return enableErrorRetry;
        }

        public bool IsVideo()
        {
            return _isVideo;
        }

        public void TakeOwnership()
        {
            
            Debug.Log($"{DEBUG_PREFIX} Take ownership (System)");
            
            if (Networking.IsOwner(gameObject))
                return;

            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }
        
        
        
        
    }
}

