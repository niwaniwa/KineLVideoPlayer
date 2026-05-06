using System;
using VRC.SDKBase;

namespace Kinel.VideoPlayer.V3.Udon.System
{
    public class KinelMediaInfo : KinelMediaBase
    {
        public void Start()
        {
        }

        public override void LoadUrl(VRCUrl url)
        {
        }

        public override bool IsValidProtocol(VRCUrl url)
        {
            return true;
        }

        public override void AddScreen()
        {
        }

        public override bool IsPlaying()
        {
            return false;
        }

        public override bool IsPaused()
        {
            return false;
        }

        public override float GetDuration()
        {
            return 0;
        }

        public override float GetTime()
        {
            return 0;
        }

        public override float GetPlaybackSpeed()
        {
            return 0;
        }

        public override void SetTime(float time)
        {
        }

        public override void ResetMedia()
        {
        }

        public override void ReloadMedia()
        {
        }

        public override void ResetScreen()
        {
        }
    }
}