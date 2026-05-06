using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDK3.Components.Video;
using VRC.SDKBase;

namespace Kinel.VideoPlayer.V3.Udon.System
{
    public abstract class KinelMediaBase : KinelModule
    {
        public KinelVideoListener VideoKinelVideoListener { get; set; }

        public VRCUrl SourceUrl { get; protected set; }

        [FormerlySerializedAs("mediaMode")] [SerializeField]
        protected KinelMediaType mediaType;


        public KinelMediaType MediaType
        {
            get => mediaType;
            protected set => mediaType = value;
        }


        public abstract void LoadUrl(VRCUrl url);

        // MediaによってサポートしているProtocolか返答する
        public abstract bool IsValidProtocol(VRCUrl url);

        public abstract void AddScreen();


        #region default video methods

        public virtual void Play()
        {
        }

        public virtual void Pause()
        {
        }

        public virtual void Stop()
        {
        }

        #endregion

        #region video status

        public abstract bool IsPlaying();
        public abstract bool IsPaused();
        public abstract float GetDuration();
        public abstract float GetTime();
        public abstract float GetPlaybackSpeed();
        public abstract void SetTime(float time);

        public virtual void SetPlaybackSpeed(float speed)
        {
        }

        public virtual void SetLoop(bool loop)
        {
        }

        public virtual bool GetLoop()
        {
            return false;
        }

        protected int resolution = 8;

        public virtual int GetResolution() => resolution;

        public virtual void SetResolution(int resolution)
        {
        }

        #endregion

        #region Video events

        public override void OnVideoStart()
        {
            Log("OnVideoStart");
            VideoKinelVideoListener.OnKinelVideoStart();
        }

        public override void OnVideoReady()
        {
            Log("OnVideoReady");
            VideoKinelVideoListener.OnKinelVideoReady();
        }

        public override void OnVideoPlay()
        {
            Log("OnVideoPlay");
            VideoKinelVideoListener.OnKinelVideoPlay();
        }

        public override void OnVideoPause()
        {
            Log("OnVideoPause");
            VideoKinelVideoListener.OnKinelVideoPause();
        }

        public override void OnVideoEnd()
        {
            Log("OnVideoEnd");
            VideoKinelVideoListener.OnKinelVideoEnd();
        }

        public override void OnVideoLoop()
        {
            Log("OnVideoLoop");
            VideoKinelVideoListener.OnKinelVideoLoop();
        }

        public override void OnVideoError(VideoError videoError)
        {
            Log("OnVideoError");
            VideoKinelVideoListener.OnKinelVideoError(videoError);
        }

        #endregion

        #region custom kinel event

        public virtual void OnKinelMediaEnabled()
        {
        }

        #endregion

        #region kinel controll methods

        public abstract void ResetMedia();

        public abstract void ReloadMedia();

        public abstract void ResetScreen();

        #endregion
    }
}