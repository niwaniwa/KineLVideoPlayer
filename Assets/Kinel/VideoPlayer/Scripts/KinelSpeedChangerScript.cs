using Kinel.VideoPlayer.Udon;
using UnityEngine;
using UnityEngine.UI;

namespace Kinel.VideoPlayer.Scripts
{
    public class KinelSpeedChangerScript : KinelScriptBase
    {

        public KinelVideoPlayer videoPlayer;
        public Animator animator;
        public Slider speedChangerSlider;
        public Text text;
        public float min, max, animationParameterMax, increaseSpeed;
        public AudioSource source;
        public bool pitchChange, isAutoFill, manual;

    }
}