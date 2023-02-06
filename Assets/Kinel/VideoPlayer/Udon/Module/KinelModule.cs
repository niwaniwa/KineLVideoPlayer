using System;
using UdonSharp;

namespace Kinel.VideoPlayer.Udon.Module
{
    public class KinelModule : UdonSharpBehaviour
    {
        
        public const string DEBUG_PREFIX = "[<color=#58ACFA>KineL</color>]";
        public const string DEBUG_ERROR_PREFIX = "[<color=#dc143c>KineL</color>]";

        public virtual void OnKinelUrlUpdate(){}
        public virtual void OnKinelVideoReady(){}
        public virtual void OnKinelVideoStart(){}
        public virtual void OnKinelVideoEnd(){}
        public virtual void OnKinelVideoPause(){}
        public virtual void OnKinelVideoLoop(){}
        public virtual void OnKinelVideoError(){}
        public virtual void OnKinelVideoReset(){}
        public virtual void OnKinelVideoRetryError(){}
        public virtual void OnKinelChangeVideoTime(){}
        public virtual void OnKinelVideoModeChange(){}
        public virtual void OnKinelVideoPlayerLocked(){}
        public virtual void OnKinelVideoPlayerUnlocked(){}

    }
}