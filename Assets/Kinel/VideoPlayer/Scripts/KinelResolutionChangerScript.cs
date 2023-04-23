using Kinel.VideoPlayer.Scripts.Parameter;
using Kinel.VideoPlayer.Udon;
using UnityEngine;
using UnityEngine.UI;
using Slider = UnityEngine.UIElements.Slider;

namespace Kinel.VideoPlayer.Scripts
{
    public class KinelResolutionChangerScript : KinelScriptBase
    {
        public KinelVideoPlayer videoPlayer;
        public Animator animator;
        public Text text;
        public int[] resolutionArray;
        public FillResult isAutoFill;

    }
}