using TMPro;
using UdonSharp;
using UnityEngine;

namespace Kinel.VideoPlayer.V3.Udon.Interface
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class KinelTooltipController : UdonSharpBehaviour
    {
        [SerializeField] private GameObject tooltipPanel;
        [SerializeField] private TMP_Text tooltipText;
        [SerializeField] private Animator tooltipAnimator;

        private const string IsVisibleParam = "IsVisible";

        public void ShowTooltip(string text, Vector3 worldPosition)
        {
            if (tooltipPanel == null || tooltipText == null) return;
            tooltipText.text = text;
            tooltipPanel.transform.position = worldPosition;
            tooltipPanel.SetActive(true);
            if (tooltipAnimator != null)
                tooltipAnimator.SetBool(IsVisibleParam, true);
        }

        public void HideTooltip()
        {
            if (tooltipPanel == null) return;
            if (tooltipAnimator != null)
                tooltipAnimator.SetBool(IsVisibleParam, false);
            else
                tooltipPanel.SetActive(false);
        }
    }
}
