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
        private int _localCurrentMode = 0;

        public int CurrentMode
        {
            get
            {
                return _currentMode;
            }
            set
            {
                _currentMode = value;
                if (_localCurrentMode != _currentMode)
                {
                    Debug.Log($"{DEBUG_PREFIX} Deserialization: Change mode");
                    ChangeMode(_currentMode);
                }
            }
        }


        public void ChangeMode(int mode)
        {
            if (_currentMode == mode)
            {
                Debug.Log($"{DEBUG_PREFIX} already set to #{mode}");
                return;
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
                    _localCurrentMode = _currentMode;
                    break;
                case STREAM_MODE:
                    if (Networking.IsOwner(Networking.LocalPlayer, videoPlayer.gameObject))
                    {
                        Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
                        videoPlayer.ResetGlobal();
                        _currentMode = mode;
                        RequestSerialization();
                    }
                    _localCurrentMode = _currentMode;

                    break;
                default:
                    break;
            }

            videoPlayer.CallEvent("OnKinelVideoModeChange");
            
        }

        public void Loop(bool loop)
        {
            unityVideoPlayer.Loop = loop;
            vrcAvProVideoPlayer.Loop = loop;
        }

        // public override void OnDeserialization()
        // {
        //     if (_localCurrentMode != _currentMode)
        //     {
        //         Debug.Log($"{DEBUG_PREFIX} Deserialization: Change mode");
        //         videoPlayer.Reset();
        //         ChangeMode(_currentMode);
        //     }
        // }

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

        public int GetCurrentVideoMode()
        {
            return _currentMode;
        }

        public override void OnVideoReady()
        {
            videoPlayer.OnVideoReady();;
        }

        public override void OnVideoStart()
        {
            videoPlayer.OnVideoStart();
        }

        public override void OnVideoEnd()
        {
            videoPlayer.OnVideoEnd();
        }

        public override void OnVideoError(VideoError videoError)
        {
            videoPlayer.OnVideoError(videoError);
        }

        public override void OnVideoLoop()
        {
            videoPlayer.OnVideoLoop();
        }

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