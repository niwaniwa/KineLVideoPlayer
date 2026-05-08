using System;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDK3.Components.Video;
using VRC.SDKBase;

namespace Kinel.VideoPlayer.V3.Udon.System
{
    public class KinelPlayerController : KinelVideoListener
    {
        // UDONSYNCEDは分離する -> to KinelVariableSyncer

        [SerializeField] private KinelMediaBase[] mediaModule;
        [SerializeField] private AudioSource audioSource;
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
                oldMedia.ResetMedia();
                NowSelectedMediaModule.OnKinelMediaEnabled();
                OnKinelMediaTypeChanged();
            }
        }

        public KinelMediaInfo NowPlayingMediaInfo { get; private set; }

        public bool Mute
        {
            get => audioSource.mute;
            set => audioSource.mute = value;
        }

        #region Media Status

        public float Volume
        {
            get => audioSource.volume;
            set => audioSource.volume = value;
        }

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
            var url = NowSelectedMediaModule.SourceUrl;
            if (url != null && !url.Equals(VRCUrl.Empty))
            {
                _isReloading = true;
            }
            NowSelectedMediaModule.SetResolution(resolution);
        }

        #endregion

        public void Start()
        {
            Initialize();
        }

        public void Initialize()
        {
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

        public void SetVolume(float volume)
        {
            audioSource.volume = Mathf.Clamp(volume, 0, 1);
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
            NowSelectedMediaModule.SetLoop(_loop);

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

        #region Loop

        private bool _loop;

        public bool Loop => _loop;

        public void SetLoop(bool loop)
        {
            _loop = loop;
            NowSelectedMediaModule.SetLoop(loop);
            foreach (var listener in Listeners)
            {
                listener.OnKinelLoopChanged(loop);
            }
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