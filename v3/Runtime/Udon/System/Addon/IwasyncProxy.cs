#if KINEL_IWASYNC
using HoshinoLabs.IwaSync3.Udon;
using Kinel.VideoPlayer.V3.Udon.System.Addon;
#endif
using Kinel.VideoPlayer.V3.Udon.Module;
using UnityEngine;
using VRC.SDKBase;
using NotImplementedException = System.NotImplementedException;

namespace Kinel.VideoPlayer.V3.Udon.System.Component
{
    public class IwasyncProxy : KinelMediaBase
    {
#if KINEL_IWASYNC
        [SerializeField] private IwaSync3 iwasync;
        [SerializeField] private VideoCore videoCore;
        [SerializeField] private uint defaultVideoMode = 0;
        [SerializeField] private KinelIwasyncBridge bridge;
        [SerializeField] private KinelIwasyncScreen screen;

        // [import]みたいな感じでBuild時に自動的に取得したいが、一旦手動で設定する
        // private VideoCore _videoCore;

        public void Start()
        {
        }

        public void SetVolume(float volume)
        {
            videoCore.volume = volume;
        }

        public override void LoadUrl(VRCUrl url)
        {
            videoCore.TakeOwnership();
            videoCore.PlayURL(defaultVideoMode, url);
            videoCore.RequestSerialization();
        }

        public override bool IsValidProtocol(VRCUrl url)
        {
            return true;
        }

        public override void AddScreen()
        {
            return;
        }

        public override bool IsPlaying()
        {
            return videoCore.isPlaying;
        }

        public override bool IsPaused()
        {
            return videoCore.paused;
        }

        public override float GetDuration()
        {
            return videoCore.duration;
        }

        public override float GetTime()
        {
            return videoCore.time;
        }

        public override float GetPlaybackSpeed()
        {
            return videoCore.speed;
        }

        public VRCUrl GetPlayingUrl()
        {
            return videoCore.url;
        }

        public override void SetTime(float time)
        {
            videoCore.time = time;
        }

        public override void ResetMedia()
        {
            // videoCore.Stop();
            // videoCore.PlayURL(0, VRCUrl.Empty);
            videoCore.RemoveListener(bridge);
            videoCore.RemoveListener(screen);
        }

        public override void ReloadMedia()
        {
            videoCore.Reload();
        }

        public override void ResetScreen()
        {
            screen.ResetScreen();
        }

        public override void OnKinelMediaEnabled()
        {
            videoCore.AddListener(bridge);
            videoCore.AddListener(screen);
        }
#endif
    }
}