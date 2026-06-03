using System;
using Kinel.VideoPlayer.V3.Udon.System.Component;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDK3.Components.Video;
using VRC.SDKBase;

namespace Kinel.VideoPlayer.V3.Udon.System
{
    public class KinelPlayerController : KinelVideoListener
    {
        [SerializeField] private KinelMediaBase[] mediaModule;
        [SerializeField] private KinelMediaType nowSelectedType;

        [SerializeField] private int maxRetryCount = 3;
        [SerializeField] private float retryIntervalSeconds = 5f;
        private int _retryCount;
        private VRCUrl _lastLoadedUrl;

        private bool _isReloading;
        public bool IsReloading => _isReloading;

        public KinelVideoListener[] Listeners { get; private set; } = Array.Empty<KinelVideoListener>();

        public KinelMediaBase NowSelectedMediaModule
        {
            get
            {
                foreach (var t in mediaModule)
                {
                    // if (t.MediaMode == KinelUtilities.ConvertToMediaMode(nowSelectedMediaIndex))
                    //     return t;
                    if (t.MediaType == nowSelectedType)
                        return t;
                }

                return mediaModule[0];
            }
        }

        public KinelMediaType NowSelectedType
        {
            get => nowSelectedType;
            set
            {
                if (nowSelectedType == value) return;
                var oldMedia = GetMediaByType(nowSelectedType);
                nowSelectedType = value;
                if (oldMedia != null) oldMedia.ResetMedia();
                NowSelectedMediaModule.OnKinelMediaEnabled();
                OnKinelMediaTypeChanged();
            }
        }

        public KinelMediaInfo NowPlayingMediaInfo { get; private set; }

        #region Media Status

        public bool IsPlaying() => NowSelectedMediaModule.IsPlaying();
        public bool IsPaused() => NowSelectedMediaModule.IsPaused();

        public bool IsStream() => float.IsInfinity(NowSelectedMediaModule.GetDuration());

        public float GetTime() => NowSelectedMediaModule.GetTime();

        public float GetDuration() => NowSelectedMediaModule.GetDuration();

        public void SetTime(float time) => NowSelectedMediaModule.SetTime(time);

        public VRCUrl GetPlayingUrl() => NowSelectedMediaModule.SourceUrl;

        public int GetResolution() => NowSelectedMediaModule.GetResolution();

        public void SetResolution(int resolution)
        {
            if (IsPlaying())
            {
                var url = NowSelectedMediaModule.SourceUrl;
                if (url != null && !url.Equals(VRCUrl.Empty))
                {
                    _isReloading = true;
                }
            }

            NowSelectedMediaModule.SetResolution(resolution);
        }

        #endregion

        public void Start()
        {
            Initialize();
            SendCustomEventDelayedFrames(nameof(_ApplyInitialLoopMode), 1);
            SendCustomEventDelayedFrames(nameof(_ApplyInitialLock), 1);
        }

        public void Initialize()
        {
            _loopMode = _initialLoopMode;
            _isLock = _initialLock;
            foreach (var module in mediaModule)
            {
                module.VideoKinelVideoListener = this;
            }
        }

        public void LoadUrl(VRCUrl url)
        {
            _retryCount = 0;
            _lastLoadedUrl = url;
            OnKinelPostUrlInput(url);
            Debug.Log($"{DebugPrefix} Now selected mode: {nowSelectedType}");
            NowSelectedMediaModule.LoadUrl(url);
        }

        public void ResetSystem()
        {
        }

        #region Listener

        public void AddListener(KinelVideoListener kinelVideoListener)
        {
            Listeners = KinelUtilities.AddArray(Listeners, kinelVideoListener);
        }

        public void RemoveListener(KinelVideoListener kinelVideoListener)
        {
            Listeners = KinelUtilities.RemoveArray(Listeners, kinelVideoListener);
        }

        #endregion

        #region video player event

        public override void OnKinelVideoStart()
        {
            Log("OnKinelVideoStart");
            if (_isReloading)
            {
                SendCustomEventDelayedFrames(nameof(_ClearReloading), 1);
            }

            foreach (var listener in Listeners)
            {
                listener.OnKinelVideoStart();
            }
        }

        public override void OnKinelVideoReady()
        {
            foreach (var listener in Listeners)
            {
                listener.OnKinelVideoReady();
            }

            NowSelectedMediaModule.Play();
        }

        public override void OnKinelVideoPlay()
        {
            foreach (var listener in Listeners)
            {
                listener.OnKinelVideoPlay();
            }
        }

        public override void OnKinelVideoPause()
        {
            foreach (var listener in Listeners)
            {
                listener.OnKinelVideoPause();
            }
        }

        public override void OnKinelVideoEnd()
        {
            foreach (var listener in Listeners)
            {
                listener.OnKinelVideoEnd();
            }
        }

        public override void OnKinelVideoLoop()
        {
            foreach (var listener in Listeners)
            {
                listener.OnKinelVideoLoop();
            }
        }

        public override void OnKinelVideoRetry()
        {
            foreach (var listener in Listeners)
            {
                listener.OnKinelVideoRetry();
            }
        }

        public override void OnKinelVideoError(VideoError videoError)
        {
            foreach (var listener in Listeners)
            {
                listener.OnKinelVideoError(videoError);
            }

            if (_retryCount < maxRetryCount && _lastLoadedUrl != null && !_lastLoadedUrl.Equals(VRCUrl.Empty))
            {
                _retryCount++;
                Log($"Retry {_retryCount}/{maxRetryCount} in {retryIntervalSeconds}s");
                SendCustomEventDelayedSeconds(nameof(RetryLoadUrl), retryIntervalSeconds);
            }
            else if (_isReloading)
            {
                _isReloading = false;
            }
        }

        public void RetryLoadUrl()
        {
            if (_lastLoadedUrl == null || _lastLoadedUrl.Equals(VRCUrl.Empty)) return;

            Log($"Retrying URL: {_lastLoadedUrl}");
            OnKinelVideoRetry();
            NowSelectedMediaModule.LoadUrl(_lastLoadedUrl);
        }

        public bool IsRetryExhausted() => _retryCount >= maxRetryCount;

        #endregion

        #region original video event

        public override void OnKinelMediaTypeChanged()
        {
            NowSelectedMediaModule.SetLoop(_loopMode == LoopMode.Single);

            foreach (var listener in Listeners)
            {
                listener.OnKinelMediaTypeChanged();
            }
        }

        /// <summary>
        /// URLロードが発火した後のタイミングで発生
        /// </summary>
        /// <param name="url"></param>
        public override void OnKinelLoadUrl(VRCUrl url)
        {
            foreach (var listener in Listeners)
            {
                listener.OnKinelLoadUrl(url);
            }
        }

        /// <summary>
        /// URL入力直後のイベント。URLInput直後に発生
        /// </summary>
        /// <param name="url"></param>
        public override void OnKinelPostUrlInput(VRCUrl url)
        {
            foreach (var listener in Listeners)
            {
                listener.OnKinelPostUrlInput(url);
            }
        }

        /// <summary>
        /// テクスチャが更新された際に呼び出される。
        /// </summary>
        /// <param name="texture">再生されている動画のテクスチャ</param>
        public override void OnKinelVideoTextureUpdated(Texture texture)
        {
            foreach (var listener in Listeners)
            {
                listener.OnKinelVideoTextureUpdated(texture);
            }
        }

        public override void OnKinelYttlDataLoaded()
        {
            foreach (var listener in Listeners)
            {
                listener.OnKinelYttlDataLoaded();
            }
        }

        public override void OnKinelMediaReset()
        {
            foreach (var listener in Listeners)
            {
                listener.OnKinelMediaReset();
            }
        }

        public override void OnKinelVideoSpeedChanged(float speed)
        {
            foreach (var listener in Listeners)
            {
                listener.OnKinelVideoSpeedChanged(speed);
            }
        }

        public override void OnKinelSeek(float time)
        {
            foreach (var listener in Listeners)
            {
                listener.OnKinelSeek(time);
            }
        }

        #endregion

        #region queue event

        public override void OnKinelQueueAdded()
        {
            foreach (var listener in Listeners)
            {
                listener.OnKinelQueueAdded();
            }
        }

        public override void OnKinelQueueRemoved()
        {
            foreach (var listener in Listeners)
            {
                listener.OnKinelQueueRemoved();
            }
        }

        public override void OnKinelQueueStart()
        {
            foreach (var listener in Listeners)
            {
                listener.OnKinelQueueStart();
            }
        }

        #endregion

        #region Loop

        [SerializeField] private LoopMode _initialLoopMode = LoopMode.None;
        private LoopMode _loopMode;

        public LoopMode LoopMode => _loopMode;

        public void SetLoopMode(LoopMode loopMode)
        {
            _loopMode = loopMode;
            NowSelectedMediaModule.SetLoop(loopMode == LoopMode.Single);
            foreach (var listener in Listeners)
            {
                listener.OnKinelLoopModeChanged(loopMode);
            }
        }

        public void _ApplyInitialLoopMode()
        {
            SetLoopMode(_loopMode);
        }

        #endregion

        #region Lock

        [SerializeField] private KinelPermissionProviderBase permissionProvider;
        [SerializeField] private bool _initialLock = false;
        private bool _isLock;

        public bool IsLock => _isLock;

        /// <summary>
        /// 指定プレイヤーが操作権限を持つかを判定
        /// 内部ロジックについてはPermissionProviderに委譲する。
        /// </summary>
        public bool CanOperate(VRCPlayerApi player)
        {
            if (permissionProvider != null)
                return permissionProvider.CanOperate(player);
            return player != null && player.IsValid() && player.isMaster;
        }

        public void SetLock(bool isLock)
        {
            _isLock = isLock;
            foreach (var listener in Listeners)
            {
                if (isLock) listener.OnKinelLocked();
                else listener.OnKinelUnlocked();
            }
        }

        public void _ApplyInitialLock()
        {
            SetLock(_isLock);
        }

        #endregion

        #region Mirror Inversion

        private bool _noMirrorInversion;

        public bool NoMirrorInversion => _noMirrorInversion;

        public void SetNoMirrorInversion(bool value)
        {
            _noMirrorInversion = value;
            foreach (var listener in Listeners)
            {
                listener.OnKinelNoMirrorInversionChanged(value);
            }
        }

        #endregion

        public override void OnKinelPlaylistActiveChanged(bool isActive)
        {
            foreach (var listener in Listeners)
            {
                listener.OnKinelPlaylistActiveChanged(isActive);
            }
        }

        /// <summary>
        /// 指定されたメディアタイプに切り替える
        /// </summary>
        public void SwitchMediaType(KinelMediaType type)
        {
            if (GetMediaByType(type) == null) return;
            NowSelectedType = type;
        }

        public void ResetMedia()
        {
            NowSelectedMediaModule.ResetMedia();
        }

        /// <summary>
        /// ローカルで再読み込みを行うメソッド（グローバル同期しない）
        /// </summary>
        public void ReloadMedia()
        {
            var url = NowSelectedMediaModule.SourceUrl;
            if (url == null || url.Equals(VRCUrl.Empty))
            {
                NowSelectedMediaModule.ReloadMedia();
                return;
            }

            _isReloading = true;
            _retryCount = 0;
            _lastLoadedUrl = url;
            NowSelectedMediaModule.ReloadMedia();
        }

        public void _ClearReloading()
        {
            _isReloading = false;
        }

        #region Playback Speed

        private float _lastSpeedChangeTime;
        private bool _speedReloadPending;
        private const float SpeedReloadDebounce = 0.4f;

        /// <summary>
        /// 再生速度を変更する集約 API。
        /// AVPro はフィールド書き込みだけでは反映されないため、再生中なら reload で適用する
        /// ただリロードしなくても反映されたので謎かも...
        /// </summary>
        public void SetPlaybackSpeed(float speed)
        {
            NowSelectedMediaModule.SetPlaybackSpeed(speed);
            if (NowSelectedMediaModule.MediaType == KinelMediaType.AvPro && IsPlaying() && !IsStream())
                ScheduleSpeedReload();
        }

        private void ScheduleSpeedReload()
        {
            _lastSpeedChangeTime = Time.time;
            if (_speedReloadPending) return;
            _speedReloadPending = true;
            SendCustomEventDelayedSeconds(nameof(_DoSpeedReload), SpeedReloadDebounce);
        }

        public void _DoSpeedReload()
        {
            if (Time.time - _lastSpeedChangeTime < SpeedReloadDebounce - 0.01f)
            {
                SendCustomEventDelayedSeconds(nameof(_DoSpeedReload), SpeedReloadDebounce);
                return;
            }

            _speedReloadPending = false;
            if (NowSelectedMediaModule.MediaType == KinelMediaType.AvPro && IsPlaying() && !IsStream())
                ReloadMedia();
        }

        #endregion

        public KinelMediaBase GetMediaByType(KinelMediaType type)
        {
            for (int i = 0; i < mediaModule.Length; i++)
            {
                if (type == mediaModule[i].MediaType)
                {
                    return mediaModule[i];
                }
            }

            return null;
        }
    }
}