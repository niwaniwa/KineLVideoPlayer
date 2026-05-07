using UnityEngine;

namespace Kinel.VideoPlayer.V3.Editor
{
    public class BaseKinelVideoPlayerEditor : BaseKinelEditor
    {
        protected static readonly string DebugLogPrefix = "[<color=#58ACFA>KineL</color><color=#ffff00>#Editor</color>]";

        protected void Log(object message) => Debug.Log($"{DebugLogPrefix} {message}");

        protected void LogWarning(object message) => Debug.LogWarning($"{DebugLogPrefix} {message}");
        public virtual void ApplyUdonProperties() {}
        
    }
}