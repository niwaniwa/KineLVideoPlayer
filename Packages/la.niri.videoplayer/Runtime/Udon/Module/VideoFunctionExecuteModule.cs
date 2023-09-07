using UdonSharp;
using UnityEngine;

namespace Kinel.VideoPlayer.Udon.Module
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class VideoFunctionExecuteModule : KinelModule
    {

        [SerializeField] private KinelVideoPlayerUI videoPlayerUI;
        [SerializeField] private string functionName;

        public void Execute()
        {
            videoPlayerUI.GetVideoPlayer().SendCustomEvent(functionName);
        }

    }
}