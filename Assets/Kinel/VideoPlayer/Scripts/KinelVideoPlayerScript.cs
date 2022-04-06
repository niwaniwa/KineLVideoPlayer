using System;
using Kinel.VideoPlayer.Udon;
using UnityEditor;
using VRC.SDKBase;

namespace Kinel.VideoPlayer.Scripts
{
    
    public class KinelVideoPlayerScript : KinelScriptBase
    {
        public KinelVideoPlayer videoPlayer;
        public bool loop, isAutoFill, enableErrorRetry, enableDefaultUrl;
        public int retryLimit;
        public float deleyLimit;
        public string defaultUrl;
        public int defaultUrlMode;
    }
}