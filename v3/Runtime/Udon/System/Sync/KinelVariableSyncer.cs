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


        [SerializeField] private KinelLocalPlayerController controller;
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
        private bool _isRemoteLoad; // 繝ｪ繝｢繝ｼ繝郁ｵｷ蝗縺ｮ繝ｭ繝ｼ繝我ｸｭ繝輔Λ繧ｰ (髱槫酔譛溷ｯｾ蠢・
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
                _isRemoteAction = true; // 蜷梧悄繧ｳ繝ｼ繝ｫ繝舌ャ繧ｯ逕ｨ LoadUrl -> OnKinelPostUrlInput繧偵ヶ繝ｭ繝・け
                _isRemoteLoad = true; // 髱槫酔譛溘さ繝ｼ繝ｫ繝舌ャ繧ｯ逕ｨ OnKinelVideoPlay/Start繧偵ヶ繝ｭ繝・け
                controller.NowSelectedType = (KinelMediaType)_syncedMediaType;
                controller.LoadUrl(_syncedUrl);
                _isRemoteAction = false; // 蜷梧悄繧ｳ繝ｼ繝ｫ繝舌ャ繧ｯ螳御ｺ・ｒ繝√ぉ繝・け
                // _isRemoteLoad縺ｯ繝ｭ繝ｼ繝牙ｮ御ｺ・う繝吶Φ繝医∪縺ｧ菫晄戟
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

            // OnKinelVideoPlay 縺悟・縺ｫ蜃ｦ逅・ｸ医∩縺ｮ蝣ｴ蜷医・繧ｹ繧ｭ繝・・ (resume譎ゅ・莠碁㍾蜃ｦ逅・亟豁｢)
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
                // Start/Play 荳｡譁ｹ縺ｧ繧ｷ繝ｼ繧ｯ(驥崎､・ｮ溯｡後・辟｡螳ｳ縲・・ｺ城撼萓晏ｭ倥・縺溘ａ)
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
                // LOADING/IDLE 竊・PLAYING: 譁ｰ隕丞・逕溘・髢句ｧ区凾蛻ｻ繧定ｨ倬鹸
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

            // 蜊ｳ譎ょ渚譏: 蜀咲函荳ｭ縺ｪ繧牙・繧ｷ繝ｼ繧ｯ
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

            // AB Loop 迥ｶ諷九ｂ繧ｹ繝翫ャ繝励す繝ｧ繝・ヨ
            if (abLoop != null)
            {
                _syncedPointA = abLoop.PointA;
                _syncedPointB = abLoop.PointB;
                _syncedABLoopEnabled = abLoop.IsABLoopEnabled;
            }

            // 繝ｫ繝ｼ繝礼憾諷九ｂ繧ｹ繝翫ャ繝励す繝ｧ繝・ヨ
            _syncedLoop = controller.Loop;
        }

        /// <summary>
        /// 迴ｾ蝨ｨ縺ｮ繧ｵ繝ｼ繝舌・譎ょ綾縺九ｉ譛溷ｾ・＆繧後ｋ蜍慕判蜀咲函菴咲ｽｮ繧定ｨ育ｮ励☆繧九・
        /// SyncedVideoStartGlobalTime 縺ｯ縲碁溷ｺｦ霎ｼ縺ｿ縺ｮ莉ｮ諠ｳ髢句ｧ区凾蛻ｻ縲・now - pos/speed) 縺ｨ縺励※險ｭ螳壹＆繧後ｋ蜑肴署縲・
        /// </summary>
        private float CalcExpectedPosition()
        {
            return (float)(Networking.GetServerTimeInSeconds() - SyncedVideoStartGlobalTime) * _syncedSpeed +
                   _localTimeOffset;
        }

        private void DriftCheck()
        {
            if (Networking.IsOwner(gameObject)) return; // owner縺悟渕貅悶↓縺ｪ繧狗ぜ

            float expected = CalcExpectedPosition();
            float actual = controller.GetTime();
            float duration = controller.GetDuration();

            // AB Loop 譛牙柑譎・ expected 繧・A-B 遽・峇縺ｫ謚倥ｊ霑斐☆
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