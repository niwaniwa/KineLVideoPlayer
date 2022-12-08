using UdonSharp;

namespace Kinel.VideoPlayer.Udon.Module
{
    public class KinelModule : UdonSharpBehaviour
    {

        public virtual void OnKinelUrlUpdate()
        {
            
        }
        public virtual void OnKinelVideoReady(){}
        public virtual void OnKinelVideoStart(){}
        public virtual void OnKinelVideoEnd(){}
        public virtual void OnKinelVideoLoop(){}
        public virtual void OnKinelVideoError(){}
        public virtual void OnKinelVideoRetryError(){}
        public virtual void OnKinelChangeVideoTime(){}
        public virtual void OnKinelVideoModeChange(){}

    }
}