using Kinel.VideoPlayer.Scripts.Parameter;
using Kinel.VideoPlayer.Udon;

namespace Kinel.VideoPlayer.Scripts
{
    public class KinelScreenScript : KinelScriptBase
    {
         public KinelVideoPlayer videoPlayer;
        
         public string propertyName;
         public string screenName;
         public int materialIndex;

         public bool mirrorInverion;
         public bool backCulling;
         public float transparency;

        public FillResult isAutoFill;
    }
}