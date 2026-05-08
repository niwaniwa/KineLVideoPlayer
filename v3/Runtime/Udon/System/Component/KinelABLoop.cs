using Kinel.VideoPlayer.V3.Udon.System;
using UdonSharp;
using UnityEngine;

namespace Kinel.VideoPlayer.V3.Udon.System.Component
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class KinelABLoop : KinelVideoListener
    {
        [SerializeField] private KinelLocalPlayerController controller;

        private float _pointA = -1f;
        private float _pointB = -1f;
        private bool _abLoopEnabled = false;
        private bool _isSeeking = false;

        private KinelVideoListener[] _listeners = new KinelVideoListener[0];

        public float PointA => _pointA;
        public float PointB => _pointB;
        public bool IsABLoopEnabled => _abLoopEnabled;

        #region Listener

        public void AddListener(KinelVideoListener listener)
        {
            _listeners = KinelUtilities.AddArray(_listeners, listener);
        }

        private void NotifyListeners()
        {
            foreach (var listener in _listeners)
            {
                listener.OnKinelABLoopStateChanged();
            }
        }

        #endregion

        #region Remote Apply (called by Syncer)

        public void ApplyRemotePointA(float value)
        {
            _pointA = value;
            NotifyListeners();
        }

        public void ApplyRemotePointB(float value)
        {
            _pointB = value;
            NotifyListeners();
        }

        public void ApplyRemoteABLoopEnabled(bool value)
        {
            _abLoopEnabled = value;
            NotifyListeners();
        }

        #endregion

        public void Start()
        {
            if (controller == null)
            {
                LogWarning("KinelABLoop: controller is null");
                return;
            }

            controller.AddListener(this);
        }

        public void FixedUpdate()
        {
            if (!_abLoopEnabled || _isSeeking) return;
            if (!controller.IsPlaying()) return;

            float currentTime = controller.GetTime();
            if (currentTime >= _pointB - 0.05f)
            {
                _isSeeking = true;
                controller.SetTime(_pointA);
                SendCustomEventDelayedFrames(nameof(_ClearSeekingFlag), 5);
            }
        }

        public void _ClearSeekingFlag()
        {
            _isSeeking = false;
        }

        public void SetPointA(float time)
        {
            if (controller.IsStream()) return;

            float duration = controller.GetDuration();
            if (duration <= 0f) return;

            time = Mathf.Clamp(time, 0f, duration);

            if (_pointB >= 0f && time >= _pointB)
                time = _pointB - 0.1f;

            if (time < 0f) time = 0f;

            _pointA = time;
            NotifyListeners();
            Log($"SetPointA: {_pointA}");
        }

        public void SetPointB(float time)
        {
            if (controller.IsStream()) return;

            float duration = controller.GetDuration();
            if (duration <= 0f) return;

            time = Mathf.Clamp(time, 0f, duration);

            if (_pointA >= 0f && time <= _pointA)
                time = _pointA + 0.1f;

            if (time > duration) time = duration;

            _pointB = time;
            NotifyListeners();
            Log($"SetPointB: {_pointB}");
        }

        public void SetPointAToCurrent()
        {
            SetPointA(controller.GetTime());
        }

        public void SetPointBToCurrent()
        {
            SetPointB(controller.GetTime());
        }

        public void EnableABLoop()
        {
            _abLoopEnabled = true;

            // ポイント未設定ならデフォルト範囲をセット
            float duration = controller.GetDuration();
            if (_pointA < 0f)
                _pointA = 0f;
            if (_pointB < 0f)
                _pointB = duration > 0f ? duration : 0f;

            NotifyListeners();
            Log("AB Loop enabled");
        }

        public void DisableABLoop()
        {
            _abLoopEnabled = false;
            NotifyListeners();
            Log("AB Loop disabled");
        }

        public void ToggleABLoop()
        {
            if (_abLoopEnabled)
                DisableABLoop();
            else
                EnableABLoop();
        }

        public void ClearABLoop()
        {
            _abLoopEnabled = false;
            _isSeeking = false;
            _pointA = -1f;
            _pointB = -1f;
            NotifyListeners();
            Log("AB Loop cleared");
        }

        #region Listener Callbacks

        public override void OnKinelVideoStart()
        {
            // start した場合でも止めたいかも
            // ClearABLoop();
        }

        public override void OnKinelMediaReset()
        {
            _pointA = -1f;
            _pointB = -1f;
            _abLoopEnabled = false;
            _isSeeking = false;
            NotifyListeners();
        }

        #endregion
    }
}
