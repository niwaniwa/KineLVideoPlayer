using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components.Video;
using VRC.SDK3.Video.Components;
using VRC.SDK3.Video.Components.AVPro;
using VRC.SDK3.Video.Components.Base;
using VRC.SDKBase;

namespace Kinel.VideoPlayer.Udon.Controller
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class KinelVideoPlayerController : UdonSharpBehaviour
    {
        
        public const string DEBUG_PREFIX = "[<color=#58ACFA>KineL</color>]";
        
        // SerializeField
        [SerializeField] private KinelVideoPlayer videoPlayer;
        
        [SerializeField] private VRCUnityVideoPlayer unityVideoPlayer;
        [SerializeField] private VRCAVProVideoPlayer vrcAvProVideoPlayer;

        [SerializeField] private Renderer unityVideoScreenSource, avProVideoScreenSource;
        
        private const int VIDEO_MODE = 0;
        private const int STREAM_MODE = 1;

        [UdonSynced, FieldChangeCallback(nameof(CurrentMode))] private int _currentMode = 0;

        public int CurrentMode
        {
            get
            {
                return _currentMode;
            }
            set
            {
                Debug.Log($"{DEBUG_PREFIX} Deserialization: Change mode");
                ChangeMode(value);
                _currentMode = value;
                videoPlayer.CallEvent("OnKinelVideoModeChange");
            }
        }

        /// <summary>
        /// ビデオプレイヤーのモードの変更をします。
        /// </summary>
        /// <param name="mode">変更するモード</param>
        /// <returns>モードが変更されたか</returns>
        public bool ChangeMode(int mode)
        {
            if (_currentMode == mode)
            {
                Debug.Log($"{DEBUG_PREFIX} already set to #{mode}");
                return false;
            }
            
            Debug.Log($"{DEBUG_PREFIX} Change mode to #{mode}");
            switch (mode)
            {
                case VIDEO_MODE:
                    if (Networking.IsOwner(Networking.LocalPlayer, videoPlayer.gameObject))
                    {
                        Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
                        videoPlayer.ResetGlobal();
                        _currentMode = mode;
                        RequestSerialization();
                    }
                    break;
                case STREAM_MODE:
                    if (Networking.IsOwner(Networking.LocalPlayer, videoPlayer.gameObject))
                    {
                        Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
                        videoPlayer.ResetGlobal();
                        _currentMode = mode;
                        RequestSerialization();
                    }

                    break;
                default:
                    break;
            }

            // Owner判定しないとFieldCallbackChangeでOwner以外が二階実行してしまうので
            if(Networking.IsOwner(Networking.LocalPlayer, videoPlayer.gameObject))
                videoPlayer.CallEvent("OnKinelVideoModeChange");
            
            return true;
        }

        public void Loop(bool loop)
        {
            unityVideoPlayer.Loop = loop;
            vrcAvProVideoPlayer.Loop = loop;
        }

        public BaseVRCVideoPlayer GetCurrentVideoPlayer()
        {
            switch (_currentMode)
            {
                case 0:
                    return unityVideoPlayer;
                case 1:
                    return vrcAvProVideoPlayer;
                default:
                    return unityVideoPlayer;
            }
        }
        
        
        public BaseVRCVideoPlayer GetUnityVideoPlayer() => unityVideoPlayer;
        public BaseVRCVideoPlayer GetAvProVideoPlayer() => vrcAvProVideoPlayer;

        public override void OnVideoReady() => videoPlayer.OnVideoReady();

        public override void OnVideoStart() => videoPlayer.OnVideoStart();

        public override void OnVideoEnd() => videoPlayer.OnVideoEnd();

        public override void OnVideoError(VideoError videoError) => videoPlayer.OnVideoError(videoError);

        public override void OnVideoLoop() => videoPlayer.OnVideoLoop();

        public Renderer GetInternalScreen(int mode)
        {
            switch (mode)
            {
                case VIDEO_MODE:
                    return unityVideoScreenSource;
                case STREAM_MODE:
                    return avProVideoScreenSource;
                default:
                    return unityVideoScreenSource;
            }
        }
        
    }
}