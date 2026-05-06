using UnityEngine;

namespace Kinel.VideoPlayer.V3.Scripts
{
    
    public class KinelScriptsModule : MonoBehaviour
    {
        protected readonly string DebugPrefix = "[<color=#58ACFA>KineL</color>]";
        
        protected void Log(object message)
        {
            Debug.Log($"{DebugPrefix} {message}");
        }
        
        protected void LogWarning(object message) => Debug.LogWarning($"{DebugPrefix} {message}");
    }
}
