using Kinel.VideoPlayer.Udon;
using UnityEngine;

namespace Kinel.VideoPlayer.Editor
{
    public class KinelEditorBase : UnityEditor.Editor
    {

        internal const string DEBUG_LOG_PREFIX = "[<color=#58ACFA>KineL</color>]";
        internal const string DEBUG_ERROR_PREFIX = "[<color=#dc143c>KineL</color>]";
        
        public override void OnInspectorGUI()
        {
            
        }

        public KinelVideoPlayer GetVideoPlayer()
        {
            return FindObjectOfType<KinelVideoPlayer>();
        }
        
        
    }
}