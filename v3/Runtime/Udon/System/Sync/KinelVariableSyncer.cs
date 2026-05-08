using Kinel.VideoPlayer.V3.Udon.System.Component;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Kinel.VideoPlayer.V3.Udon.System.Sync
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class KinelVariableSyncer : KinelVideoListener
    {
        protected new const string DebugPrefix = "[<color=#f0e68c>KineL</color>]";


        [SerializeField] private KinelPlayerController controller;
        [SerializeField] private KinelABLoop abLoop;
        [SerializeField] private float syncInterval = 5.0f;
        [SerializeField] private float driftThreshold = 1.0f;

        // --- Synced variables ---
        [UdonSynced, FieldChangeCallback(nameof(SyncedUrl))]
        private VRCUrl _syncedUrl = VRCUrl.Empty;

        [UdonSynced, FieldChangeCallback(nameof(SyncedVideoStartGlobalTime))]
        private double _syncedVideoStartGlobalTime;

        [UdonSynced, FieldChangeCallback(nameof(SyncedPausedTime))]
        private double _syncedPausedTime;

        [UdonSynced, FieldChangeCallback(nameof(SyncedState))]
        private int _syncedState; // 0=Idle, 1=Playing, 2=Paused, 3=Loading

        [UdonSynced, FieldChangeCallback(nameof(SyncedMediaType))]
        private int _syncedMediaType = 1;

        [UdonSynced, FieldChangeCallback(nameof(SyncedSpeed))]
        private float _syncedSpeed = 1.0f;

        // --- AB Loop synced fields ---
        [UdonSynced, FieldChangeCallback(nameof(SyncedPointA))]
        private float _syncedPointA = -1f;

        [UdonSynced, FieldChangeCallback(nameof(SyncedPointB))]
        private float _syncedPointB = -1f;

        [UdonSynced, FieldChangeCallback(nameof(SyncedABLoopEnabled))]
        private bool _syncedABLoopEnabled;

        [UdonSynced, FieldChangeCallback(nameof(SyncedLoop))]
        private bool _syncedLoop;

        // --- Local state ---
        private bool _isRemoteAction;
        private bool _isRemoteLoad; // リモート起因のロード中フラグ (非同期対応用)
        private float _lastSyncCheckTime;
        private float _localTimeOffset = 0f;

        public float LocalTimeOffset => _localTimeOffset;

        private const int STATE_IDLE = 0;
        private const int STATE_PLAYING = 1;
        private const int STATE_PAUSED = 2;
        private const int STATE_LOADING = 3;

        #region FieldChangeCallbacks

        public VRCUrl SyncedUrl
        {
            get => _syncedUrl;
            set
            {
                _syncedUrl = value;
                if (_syncedUrl == null || _syncedUrl.Equals(VRCUrl.Empty)) return;
                if (Networking.IsOwner(gameObject)) return;

                Log($"Remote URL received: {_syncedUrl}");
                _isRemoteAction = true; // 同期コールバック用 LoadUrl -> OnKinelPostUrlInput をブロック
                _isRemoteLoad = true; // 非同期コールバック用 OnKinelVideoPlay/Start をブロック
                controller.NowSelectedType = (KinelMediaType)_syncedMediaType;
                controller.LoadUrl(_syncedUrl);
                _isRemoteAction = false; // 同期コールバック完了をチェック
                // _isRemoteLoad はロード完了イベントまで保持
            }
        }

        public double SyncedVideoStartGlobalTime
        {
            get => _syncedVideoStartGlobalTime;
            set
            {
                _syncedVideoStartGlobalTime = value;
                Log(
                    $"SyncedVideoStartGlobalTime setter: value={value}, IsOwner={Networking.IsOwner(gameObject)}, state={_syncedState}, IsPlaying={controller.IsPlaying()}, frame={Time.frameCount}");
                if (Networking.IsOwner(gameObject)) return;

                if (!controller.IsStream())
                {
                    if (_syncedState == STATE_PLAYING || _syncedState == STATE_PAUSED)
                    {
                        float expected = CalcExpectedPosition();
                        float duration = controller.GetDuration();
                        controller.SetTime(Mathf.Clamp(expected, 0, duration));
                        Log($"Remote video start time received: {value}, expected: {expected}, duration: {duration}");
                    }
                }
            }
        }

        public double SyncedPausedTime
        {
            get => _syncedPausedTime;
            set => _syncedPausedTime = value;
        }

        public int SyncedState
        {
            get => _syncedState;
            set
            {
                int oldState = _syncedState;
                _syncedState = value;
                Log(
                    $"SyncedState setter: {oldState} -> {_syncedState}, IsOwner={Networking.IsOwner(gameObject)}, SyncedVideoStartGlobalTime={_syncedVideoStartGlobalTime}, frame={Time.frameCount}");
                if (Networking.IsOwner(gameObject)) return;

                if (_syncedState == STATE_PAUSED && oldState == STATE_PLAYING)
                {
                    _isRemoteAction = true;
                    controller.NowSelectedMediaModule.Pause();
                    _isRemoteAction = false;
                }
                else if (_syncedState == STATE_PLAYING && oldState == STATE_PAUSED)
                {
                    _isRemoteAction = true;
                    controller.NowSelectedMediaModule.Play();
                    _isRemoteAction = false;

                    if (!controller.IsStream())
                    {
                        float expected = CalcExpectedPosition();
                        float duration = controller.GetDuration();
                        controller.SetTime(Mathf.Clamp(expected, 0, duration));
                        Log(
                            $"Remote resume: expected={expected:F2}, duration={duration:F2}, syncedStartTime={_syncedVideoStartGlobalTime}");
                    }
                }
                else if (_syncedState == STATE_IDLE && oldState != STATE_IDLE)
                {
                    _isRemoteAction = true;
                    controller.ResetMedia();
                    _isRemoteAction = false;
                }
            }
        }

        public int SyncedMediaType
        {
            get => _syncedMediaType;
            set
            {
                _syncedMediaType = value;
                if (Networking.IsOwner(gameObject)) return;

                var newType = (KinelMediaType)_syncedMediaType;
                if (controller.NowSelectedType != newType)
                {
                    _isRemoteAction = true;
                    controller.NowSelectedType = newType;
                    _isRemoteAction = false;
                }
            }
        }

        public float SyncedSpeed
        {
            get => _syncedSpeed;
            set
            {
                _syncedSpeed = value;
                if (Networking.IsOwner(gameObject)) return;

                Log($"Remote speed changed: {_syncedSpeed}");
                _isRemoteAction = true;
                controller.NowSelectedMediaModule.SetPlaybackSpeed(_syncedSpeed);
                _isRemoteAction = false;
            }
        }

        public float SyncedPointA
        {
            get => _syncedPointA;
            set
            {
                _syncedPointA = value;
                if (abLoop == null) return;

                if (Networking.IsOwner(gameObject)) return;
                abLoop.ApplyRemotePointA(_syncedPointA);
            }
        }

        public float SyncedPointB
        {
            get => _syncedPointB;
            set
            {
                _syncedPointB = value;
                if (abLoop == null) return;

                if (Networking.IsOwner(gameObject)) return;
                abLoop.ApplyRemotePointB(_syncedPointB);
            }
        }

        public bool SyncedABLoopEnabled
        {
            get => _syncedABLoopEnabled;
            set
            {
                _syncedABLoopEnabled = value;
                if (abLoop == null) return;

                if (Networking.IsOwner(gameObject)) return;
                abLoop.ApplyRemoteABLoopEnabled(_syncedABLoopEnabled);
            }
        }

        public bool SyncedLoop
        {
            get => _syncedLoop;
            set
            {
                _syncedLoop = value;
                if (Networking.IsOwner(gameObject)) return;

                _isRemoteAction = true;
                controller.SetLoop(_syncedLoop);
                _isRemoteAction = false;
            }
        }

        #endregion

        #region Lifecycle

        public void Start()
        {
            controller.AddListener(this);
            if (abLoop != null)
                abLoop.AddListener(this);
        }

        public void Update()
        {
            if (SyncedState == STATE_PLAYING && controller.IsPlaying() && !controller.IsStream())
            {
                if (Time.time - _lastSyncCheckTime >= syncInterval)
                {
                    _lastSyncCheckTime = Time.time;
                    DriftCheck();
                }
            }
        }

        #endregion

        #region Listener Events

        public override void OnKinelPostUrlInput(VRCUrl url)
        {
            if (_isRemoteAction) return;
            if (!EnsureOwnership()) return;

            SyncedUrl = url;
            SyncedMediaType = (int)controller.NowSelectedType;
            SyncedState = STATE_LOADING;
            RequestSerialization();
        }

        public override void OnKinelVideoStart()
        {
            Log(
                $"OnKinelVideoStart: IsOwner={Networking.IsOwner(gameObject)}, IsRemoteAction={_isRemoteAction}, IsRemoteLoad={_isRemoteLoad}, SyncedState={_syncedState}, SyncedVideoStartGlobalTime={_syncedVideoStartGlobalTime}, frame={Time.frameCount}");

            if (_isRemoteAction) return;

            if (_isRemoteLoad)
            {
                if (!controller.IsStream() && _syncedState == STATE_PLAYING)
                {
                    float expected = CalcExpectedPosition();
                    float duration = controller.GetDuration();
                    controller.SetTime(Mathf.Clamp(expected, 0, duration));
                    Log($"Remote load seek: expected={expected:F2}, duration={duration:F2}");
                }

                SendCustomEventDelayedFrames(nameof(_ClearRemoteLoad), 1);
                return;
            }

            if (controller.IsReloading) return;

            if (!Networking.IsOwner(gameObject)) return;

            // OnKinelVideoPlay が先に処理済みの場合はスキップ (resume時の二重処理回避用)
            if (SyncedState == STATE_PLAYING)
            {
                Log($"OnKinelVideoStart: already PLAYING, skipping (handled by OnKinelVideoPlay)");
                return;
            }

            SyncedVideoStartGlobalTime = Networking.GetServerTimeInSeconds();
            SyncedState = STATE_PLAYING;
            RequestSerialization();
            Log($"OnKinelVideoStart: fresh start, SyncedVideoStartGlobalTime={_syncedVideoStartGlobalTime}");
        }

        public override void OnKinelVideoPlay()
        {
            Log(
                $"OnKinelVideoPlay: IsOwner={Networking.IsOwner(gameObject)}, IsRemoteAction={_isRemoteAction}, IsRemoteLoad={_isRemoteLoad}, SyncedState={_syncedState}, SyncedVideoStartGlobalTime={_syncedVideoStartGlobalTime}, frame={Time.frameCount}");

            if (_isRemoteAction)
            {
                Log($"Remote video play ignored");
                return;
            }

            if (_isRemoteLoad)
            {
                // Start/Play 両方でシーク (重複実行は問題なし。状態非依存のため)
                if (!controller.IsStream() && _syncedState == STATE_PLAYING)
                {
                    float expected = CalcExpectedPosition();
                    float duration = controller.GetDuration();
                    controller.SetTime(Mathf.Clamp(expected, 0, duration));
                    Log($"Remote load seek (from Play): expected={expected:F2}, duration={duration:F2}");
                }

                SendCustomEventDelayedFrames(nameof(_ClearRemoteLoad), 1);
                return;
            }

            if (controller.IsReloading) return;

            EnsureOwnership();

            if (SyncedState == STATE_PAUSED)
            {
                float currentPos = controller.GetTime();
                SyncedVideoStartGlobalTime = Networking.GetServerTimeInSeconds() - currentPos / _syncedSpeed;
                Log($"OnKinelVideoPlay: resume correction, SyncedVideoStartGlobalTime={_syncedVideoStartGlobalTime}");
            }
            else if (SyncedState != STATE_PLAYING)
            {
                // LOADING/IDLE から PLAYING: 新規再生の開始時刻を記録
                SyncedVideoStartGlobalTime = Networking.GetServerTimeInSeconds();
                Log($"OnKinelVideoPlay: fresh play, SyncedVideoStartGlobalTime={_syncedVideoStartGlobalTime}");
            }

            SyncedState = STATE_PLAYING;
            RequestSerialization();

            Log("Synced: VideoPlay (resume)");
        }

        public override void OnKinelVideoPause()
        {
            if (_isRemoteAction)
            {
                Log($"Remote video pause ignored");
                return;
            }

            EnsureOwnership();

            SyncedPausedTime = Networking.GetServerTimeInSeconds();
            SyncedState = STATE_PAUSED;
            RequestSerialization();

            Log("Synced: VideoPause");
        }

        public override void OnKinelVideoEnd()
        {
            if (_isRemoteAction)
            {
                return;
            }

            if (!Networking.IsOwner(gameObject)) return;
            if (controller.IsReloading) return;

            SyncedState = STATE_IDLE;
            RequestSerialization();

            Log("Synced: VideoEnd");
        }

        public override void OnKinelVideoLoop()
        {
            if (_isRemoteAction)
            {
                return;
            }

            if (!Networking.IsOwner(gameObject)) return;

            SyncedVideoStartGlobalTime = Networking.GetServerTimeInSeconds();
            RequestSerialization();

            Log("Synced: VideoLoop");
        }

        public override void OnKinelVideoError(VRC.SDK3.Components.Video.VideoError videoError)
        {
            if (_isRemoteLoad && controller.IsRetryExhausted())
            {
                _isRemoteLoad = false;
                Log("Remote load flag cleared (retries exhausted)");
            }

            if (_isRemoteAction) return;
            if (!Networking.IsOwner(gameObject)) return;

            if (controller.IsRetryExhausted())
            {
                SyncedState = STATE_IDLE;
                RequestSerialization();
                Log("Retry exhausted, synced state -> IDLE");
            }
        }

        public override void OnKinelMediaTypeChanged()
        {
            if (_isRemoteAction) return;
            EnsureOwnership();

            SyncedMediaType = (int)controller.NowSelectedType;
            RequestSerialization();
        }

        public override void OnKinelVideoSpeedChanged(float speed)
        {
            if (_isRemoteAction)
            {
                return;
            }

            EnsureOwnership();

            float currentPos = controller.GetTime();
            SyncedVideoStartGlobalTime = Networking.GetServerTimeInSeconds() - currentPos / speed;
            SyncedSpeed = speed;
            RequestSerialization();

            Log($"Synced: SpeedChanged to {speed}");
        }

        public override void OnKinelMediaReset()
        {
            if (_isRemoteAction)
            {
                return;
            }

            EnsureOwnership();

            SyncedState = STATE_IDLE;
            SyncedUrl = VRCUrl.Empty;
            RequestSerialization();
        }

        public override void OnKinelABLoopStateChanged()
        {
            if (abLoop == null) return;
            if (!Networking.IsOwner(gameObject)) return;

            _syncedPointA = abLoop.PointA;
            _syncedPointB = abLoop.PointB;
            _syncedABLoopEnabled = abLoop.IsABLoopEnabled;
            RequestSerialization();
        }

        public override void OnKinelSeek(float time)
        {
            if (_isRemoteAction) return;
            if (!EnsureOwnership()) return;

            SyncedVideoStartGlobalTime = Networking.GetServerTimeInSeconds() - time / _syncedSpeed;

            if (_syncedState == STATE_PAUSED)
            {
                SyncedPausedTime = Networking.GetServerTimeInSeconds();
            }

            RequestSerialization();
            Log($"Synced: Seek, SyncedVideoStartGlobalTime: {SyncedVideoStartGlobalTime}, time: {time}");
        }

        public override void OnKinelLoopChanged(bool loop)
        {
            if (_isRemoteAction) return;
            EnsureOwnership();

            SyncedLoop = loop;
            RequestSerialization();

            Log($"Synced: LoopChanged to {loop}");
        }

        #endregion

        #region VRChat Callbacks

        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {
            if (!player.isLocal) return;

            Log("Ownership transferred to local player, snapshotting state");
            SnapshotCurrentState();
            RequestSerialization();
        }

        #endregion

        public void _ClearRemoteLoad()
        {
            _isRemoteLoad = false;
            Log("Remote load flag cleared");
        }

        #region Local Time Offset

        public void SetLocalTimeOffset(float offset)
        {
            _localTimeOffset = Mathf.Clamp(offset, -5f, 5f);
            Log($"LocalTimeOffset set to {_localTimeOffset:F1}");

            if (SyncedState == STATE_PLAYING && controller.IsPlaying() &&
                !controller.IsStream())
            {
                float expected = CalcExpectedPosition();
                float duration = controller.GetDuration();
                controller.SetTime(Mathf.Clamp(expected, 0, duration));
            }
        }

        #endregion

        #region Private Methods

        private bool EnsureOwnership()
        {
            if (!Networking.IsOwner(gameObject))
            {
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
            }

            Log("Ownership ensured");

            return true;
        }

        private void SnapshotCurrentState()
        {
            SyncedMediaType = (int)controller.NowSelectedType;
            SyncedUrl = controller.GetPlayingUrl();
            SyncedSpeed = controller.NowSelectedMediaModule.GetPlaybackSpeed();

            if (controller.IsPlaying())
            {
                SyncedState = STATE_PLAYING;
                SyncedVideoStartGlobalTime = Networking.GetServerTimeInSeconds() - controller.GetTime() / _syncedSpeed;
            }
            else if (controller.IsPaused())
            {
                SyncedState = STATE_PAUSED;
                SyncedPausedTime = Networking.GetServerTimeInSeconds();
                SyncedVideoStartGlobalTime = Networking.GetServerTimeInSeconds() - controller.GetTime() / _syncedSpeed;
            }
            else
            {
                SyncedState = STATE_IDLE;
            }

            if (abLoop != null)
            {
                _syncedPointA = abLoop.PointA;
                _syncedPointB = abLoop.PointB;
                _syncedABLoopEnabled = abLoop.IsABLoopEnabled;
            }

            _syncedLoop = controller.Loop;
        }

        /// <summary>
        /// 現在のサーバの時刻から期待される動画再生位置を計算する。
        /// SyncedVideoStartGlobalTime は「速度込みの仮想開始時刻」(now - pos/speed) として設定される前提。
        /// </summary>
        private float CalcExpectedPosition()
        {
            return (float)(Networking.GetServerTimeInSeconds() - SyncedVideoStartGlobalTime) * _syncedSpeed +
                   _localTimeOffset;
        }

        private void DriftCheck()
        {
            if (Networking.IsOwner(gameObject)) return; // owner が基準になる為

            float expected = CalcExpectedPosition();
            float actual = controller.GetTime();
            float duration = controller.GetDuration();

            // AB Loop 有効時、expected を A-B 範囲に折り返す
            if (abLoop != null && abLoop.IsABLoopEnabled
                               && abLoop.PointA >= 0f && abLoop.PointB > abLoop.PointA)
            {
                float loopLen = abLoop.PointB - abLoop.PointA;
                if (loopLen > 0f && expected > abLoop.PointB)
                {
                    expected = abLoop.PointA + ((expected - abLoop.PointA) % loopLen);
                }
            }

            if (Mathf.Abs(expected - actual) > driftThreshold)
            {
                Log($"Drift detected: expected={expected:F2}, actual={actual:F2}, correcting");
                controller.SetTime(Mathf.Clamp(expected, 0, duration));
            }
        }

        #endregion
    }
}