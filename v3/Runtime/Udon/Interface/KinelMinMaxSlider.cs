using Kinel.VideoPlayer.V3.Scripts.Attribute;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;

namespace Kinel.VideoPlayer.V3.Udon.Module
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class KinelMinMaxSlider : UdonSharpBehaviour
    {
        [SerializeField, KinelUIEvent(nameof(OnSliderAChanged), UIEventType.SliderChanged)]
        private Slider _sliderA;

        [SerializeField, KinelUIEvent(nameof(OnSliderBChanged), UIEventType.SliderChanged)]
        private Slider _sliderB;

        [SerializeField] private RectTransform _fill;

        private float _minValue;
        private float _maxValue;
        private float _leftValue;
        private float _rightValue;
        private bool _isSyncing;

        private UdonSharpBehaviour _callback;
        private string _eventLeft;
        private string _eventRight;

        public float LeftValue
        {
            get => _leftValue;
            set
            {
                _leftValue = Mathf.Clamp(value, _minValue, _rightValue);
                _isSyncing = true;
                _sliderA.value = _leftValue;
                _isSyncing = false;
                UpdateVisuals();
            }
        }

        public float RightValue
        {
            get => _rightValue;
            set
            {
                _rightValue = Mathf.Clamp(value, _leftValue, _maxValue);
                _isSyncing = true;
                _sliderB.value = _rightValue;
                _isSyncing = false;
                UpdateVisuals();
            }
        }

        public void SetUp(UdonSharpBehaviour callback, string eventLeft, string eventRight,
            float min, float max)
        {
            _callback = callback;
            _eventLeft = eventLeft;
            _eventRight = eventRight;
            SetMinMax(min, max);
        }

        public void SetLeftValueWithoutNotify(float value)
        {
            _leftValue = Mathf.Clamp(value, _minValue, _maxValue);
            _isSyncing = true;
            _sliderA.value = _leftValue;
            _isSyncing = false;
            UpdateVisuals();
        }

        public void SetRightValueWithoutNotify(float value)
        {
            _rightValue = Mathf.Clamp(value, _leftValue, _maxValue);
            _isSyncing = true;
            _sliderB.value = _rightValue;
            _isSyncing = false;
            UpdateVisuals();
        }

        public void SetMinMax(float min, float max)
        {
            _isSyncing = true;
            _minValue = min;
            _maxValue = max;
            _sliderA.minValue = min;
            _sliderA.maxValue = max;
            _sliderB.minValue = min;
            _sliderB.maxValue = max;
            _leftValue = Mathf.Clamp(_leftValue, min, max);
            _rightValue = Mathf.Clamp(_rightValue, min, max);
            _sliderA.value = _leftValue;
            _sliderB.value = _rightValue;
            _isSyncing = false;
            UpdateVisuals();
        }

        #region Slider Events

        public void OnSliderAChanged()
        {
            if (_isSyncing) return;
            _isSyncing = true;

            float newVal = _sliderA.value;
            _leftValue = Mathf.Min(newVal, _rightValue);
            if (!Mathf.Approximately(_sliderA.value, _leftValue))
                _sliderA.value = _leftValue;

            UpdateVisuals();
            _isSyncing = false;

            if (_callback != null && !string.IsNullOrEmpty(_eventLeft))
                _callback.SendCustomEvent(_eventLeft);
        }

        public void OnSliderBChanged()
        {
            if (_isSyncing) return;
            _isSyncing = true;

            float newVal = _sliderB.value;
            _rightValue = Mathf.Max(newVal, _leftValue);
            if (!Mathf.Approximately(_sliderB.value, _rightValue))
                _sliderB.value = _rightValue;

            UpdateVisuals();
            _isSyncing = false;

            if (_callback != null && !string.IsNullOrEmpty(_eventRight))
                _callback.SendCustomEvent(_eventRight);
        }

        #endregion

        #region Internal

        private void UpdateVisuals()
        {
            if (_fill == null) return;
            float range = _maxValue - _minValue;
            if (range <= 0f) return;

            float normLeft = (_leftValue - _minValue) / range;
            float normRight = (_rightValue - _minValue) / range;

            _fill.anchorMin = new Vector2(normLeft, _fill.anchorMin.y);
            _fill.anchorMax = new Vector2(normRight, _fill.anchorMax.y);
            _fill.offsetMin = new Vector2(0, _fill.offsetMin.y);
            _fill.offsetMax = new Vector2(0, _fill.offsetMax.y);
        }

        #endregion
    }
}
