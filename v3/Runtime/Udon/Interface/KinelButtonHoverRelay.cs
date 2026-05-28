using Kinel.VideoPlayer.V3.Udon.System;
using Kinel.VideoPlayer.V3.Udon.System.Component;
using UdonSharp;
using UnityEngine;

namespace Kinel.VideoPlayer.V3.Udon.Interface
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class KinelButtonHoverRelay : KinelSystemBase
    {
        [SerializeField] private KinelTooltipController tooltipController;
        [SerializeField] private string description = "";
        [SerializeField] private Vector3 tooltipOffset = new Vector3(0f, 0.15f, 0f);
        [SerializeField] private KinelAudioManager audioManager;
        [SerializeField] private AudioClip hoverSfxClip;

        public void OnHoverEnter()
        {
            Log("OnHoverEnter");

            if (tooltipController != null && !string.IsNullOrEmpty(description))
                tooltipController.ShowTooltip(description, transform.position + tooltipOffset);

            if (audioManager != null && hoverSfxClip != null)
                audioManager.PlaySE(hoverSfxClip, 0.5f);
        }

        public void OnHoverExit()
        {
            Log("OnHoverExit");
            if (tooltipController != null)
                tooltipController.HideTooltip();
        }
    }
}