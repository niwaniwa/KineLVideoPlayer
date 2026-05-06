using System;
using Kinel.VideoPlayer.V3.Scripts.Attribute;
using Kinel.VideoPlayer.V3.Udon.System;
using Kinel.VideoPlayer.V3.Udon.System.Component;
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;

namespace Kinel.VideoPlayer.V3.Udon.Module
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class KinelABLoopUI : KinelVideoListener
    {
        [SerializeField] private KinelABLoop abLoop;
        [SerializeField] private KinelLocalPlayerController controller;
        [SerializeField] private KinelMinMaxSlider rangeSlider;

        [Header("Buttons")] [SerializeField, KinelUIEvent(nameof(OnSetPointA), UIEventType.ButtonClick)]
        private Button setPointAButton;

        [SerializeField, KinelUIEvent(nameof(OnSetPointB), UIEventType.ButtonClick)]
        private Button setPointBButton;

        [SerializeField, KinelUIEvent(nameof(OnToggleABLoop), UIEventType.ButtonClick)]
        private Button toggleButton;

        [SerializeField, KinelUIEvent(nameof(OnClearABLoop), UIEventType.ButtonClick)]
        private Button clearButton;

        [Header("Display")] [SerializeField] private TMP_Text pointAText;
        [SerializeField] private TMP_Text pointBText;
        [SerializeField] private GameObject abLoopActiveIndicator;

        private bool _isDraggingLeft = false;
        private bool _isDraggingRight = false;

        public void Start()
        {
            if (controller == null || abLoop == null)
            {
                LogWarning("KinelABLoopUI: controller or abLoop is null");
                return;
            }

            controller.AddListener(this);
            abLoop.AddListener(this);

            if (rangeSlider != null)
            {
                rangeSlider.SetUp(this, nameof(OnLeftSliderDrop), nameof(OnRightSliderDrop), 0f, 0f);
                rangeSlider.gameObject.SetActive(abLoop.IsABLoopEnabled);
            }

            UpdateUI();
        }

        #region Button Events

        public void OnSetPointA()
        {
            abLoop.SetPointAToCurrent();
            SyncSliderFromModel();
            UpdateUI();
        }

        public void OnSetPointB()
        {
            abLoop.SetPointBToCurrent();
            SyncSliderFromModel();
            UpdateUI();
        }

        public void OnToggleABLoop()
        {
            abLoop.ToggleABLoop();
            SyncSliderFromModel();
            UpdateUI();
        }

        public void OnClearABLoop()
        {
            abLoop.ClearABLoop();
            SyncSliderFromModel();
            UpdateUI();
        }

        #endregion

        #region Slider Events

        public void OnLeftSliderDrag()
        {
            _isDraggingLeft = true;
        }

        public void OnRightSliderDrag()
        {
            _isDraggingRight = true;
        }

        public void OnLeftSliderDrop()
        {
            if (rangeSlider == null) return;
            abLoop.SetPointA(rangeSlider.LeftValue);
            UpdateUI();
            SendCustomEventDelayedFrames(nameof(_ClearLeftDragFlag), 5);
        }

        public void OnRightSliderDrop()
        {
            if (rangeSlider == null) return;
            abLoop.SetPointB(rangeSlider.RightValue);
            UpdateUI();
            SendCustomEventDelayedFrames(nameof(_ClearRightDragFlag), 5);
        }

        public void _ClearLeftDragFlag()
        {
            _isDraggingLeft = false;
        }

        public void _ClearRightDragFlag()
        {
            _isDraggingRight = false;
        }

        #endregion

        #region Listener Callbacks

        public override void OnKinelVideoStart()
        {
            float duration = controller.GetDuration();
            if (duration <= 0f)
            {
                SendCustomEventDelayedFrames(nameof(_UpdateSliderRangeDelayed), 5);
            }
            else
            {
                UpdateSliderRange(duration);
            }

            UpdateUI();
        }

        public void _UpdateSliderRangeDelayed()
        {
            float duration = controller.GetDuration();
            UpdateSliderRange(duration);
            UpdateUI();
        }

        public override void OnKinelMediaReset()
        {
            if (rangeSlider != null)
            {
                rangeSlider.SetMinMax(0f, 0f);
                rangeSlider.SetLeftValueWithoutNotify(0f);
                rangeSlider.SetRightValueWithoutNotify(0f);
            }

            UpdateUI();
        }

        #endregion

        #region AB Loop Listener

        public override void OnKinelABLoopStateChanged()
        {
            SyncSliderFromModel();
            UpdateUI();
        }

        #endregion

        #region Internal

        private void UpdateSliderRange(float duration)
        {
            if (rangeSlider == null) return;
            rangeSlider.SetMinMax(0f, duration);
            rangeSlider.SetLeftValueWithoutNotify(0f);
            rangeSlider.SetRightValueWithoutNotify(duration);
        }

        private void SyncSliderFromModel()
        {
            if (rangeSlider == null) return;

            if (abLoop.PointA >= 0f)
                rangeSlider.SetLeftValueWithoutNotify(abLoop.PointA);
            else
                rangeSlider.SetLeftValueWithoutNotify(rangeSlider.LeftValue);

            if (abLoop.PointB >= 0f)
                rangeSlider.SetRightValueWithoutNotify(abLoop.PointB);
            else
                rangeSlider.SetRightValueWithoutNotify(rangeSlider.RightValue);
        }

        private void UpdateUI()
        {
            // Point A text
            if (pointAText != null)
            {
                pointAText.text = abLoop.PointA >= 0f
                    ? $"A: {FormatTime(abLoop.PointA)}"
                    : "A: --:--";
            }

            // Point B text
            if (pointBText != null)
            {
                pointBText.text = abLoop.PointB >= 0f
                    ? $"B: {FormatTime(abLoop.PointB)}"
                    : "B: --:--";
            }

            // Slider visibility
            if (rangeSlider != null)
            {
                rangeSlider.gameObject.SetActive(abLoop.IsABLoopEnabled);
                Log($"set rangeslider active: {abLoop.IsABLoopEnabled}");
            }

            // Active indicator
            if (abLoopActiveIndicator != null)
            {
                abLoopActiveIndicator.SetActive(abLoop.IsABLoopEnabled);
            }
        }

        private string FormatTime(float seconds)
        {
            if (seconds < 0f) return "--:--";
            var ts = TimeSpan.FromSeconds(seconds);
            return seconds >= 3600f
                ? ts.ToString("hh\\:mm\\:ss")
                : ts.ToString("mm\\:ss");
        }

        #endregion
    }
}