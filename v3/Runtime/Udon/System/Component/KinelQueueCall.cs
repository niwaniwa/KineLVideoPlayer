using Kinel.VideoPlayer.V3.Udon.Interface;

namespace Kinel.VideoPlayer.V3.Udon.System.Component
{
    public class KinelQueueCall : KinelVideoListener
    {
        public KinelUIController UiController { get; set; }
        public int Index { get; set; }

        /// <summary>
        /// queue 行クリックで該当 index を再生する (UiController 経由)。
        /// </summary>
        public void OnQueueClick()
        {
            if (UiController != null)
            {
                UiController.OnQueuePlayByIndex(transform.GetSiblingIndex());
            }
        }

        /// <summary>
        /// queue 行の削除ボタン。
        /// UiController に index を渡して queue 削除と UI 行破棄を任せる
        /// </summary>
        public void OnQueueRemove()
        {
            if (UiController != null)
            {
                UiController.OnQueueRemoveByIndex(transform.GetSiblingIndex());
            }
        }
    }
}
