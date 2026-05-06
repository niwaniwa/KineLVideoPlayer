using Kinel.VideoPlayer.V3.Scripts.Attribute;
using Kinel.VideoPlayer.V3.Udon.System;
using UnityEngine;
using UnityEngine.UI;

namespace Kinel.VideoPlayer.V3.Udon.Test
{
    public class UdonUIEventAttachTest : KinelSystemBase
    {
        [SerializeField, KinelUIEvent(nameof(OnClick), UIEventType.ButtonClick)]
        private Button button;

        [SerializeField, KinelUIEvent(nameof(OnToggle), UIEventType.ToggleChanged)]
        private Toggle toggle;

        [SerializeField, KinelUIEvent(nameof(OnSlider), UIEventType.SliderChanged)]
        private Slider slider;

        public void OnClick()
        {
            Log("Button clicked.");
        }

        public void OnSlider()
        {
            Log($"Slider changed. {slider.value}");
        }

        public void OnToggle()
        {
            Log($"toggle changed. {toggle.isOn}");
        }
    }
}