using Kinel.VideoPlayer.V3.Udon.Interface;
using UnityEngine;
using UnityEngine.UI;

namespace Kinel.VideoPlayer.V3.Udon.System.Component
{
    public class KinelQueueCall : KinelVideoListener
    {
        public KinelUIController UiController { get; set; }

        public int Index { get; set; }

        public void Start()
        {
        }

        public void OnQueueClick()
        {
            if (UiController != null)
            {
                UiController.OnQueuePlayByIndex(transform.GetSiblingIndex());
            }
        }

        public void OnQueueRemove()
        {
        }
    }
}