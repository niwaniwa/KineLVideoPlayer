using UdonSharp;
using UnityEngine;

namespace Kinel.VideoPlayer.Udon.Module
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class KinelScreenModule : KinelModule
    {
        [SerializeField] public KinelVideoPlayer videoPlayer;
        
        [SerializeField] private string propertyName;
        [SerializeField] private string screenName;
        [SerializeField] private int materialIndex;

        [SerializeField] private bool mirrorInverion;
        [SerializeField] private bool backCulling;
        [SerializeField] private float transparency;

        private MaterialPropertyBlock _propertyBlock, _propertyBlockInternal;
        private Renderer _screenRenderer, _internalVideoRenderer, _internalAvProRenderer;

        private Texture _mainTex;
        private bool isMirrorInversion = false;

        public bool IsMirrorInversion
        {
            get => isMirrorInversion;
            set => isMirrorInversion = value;
        }
        
        public void Start()
        {
            _propertyBlock = new MaterialPropertyBlock();
            _propertyBlockInternal = new MaterialPropertyBlock();
            _screenRenderer = gameObject.transform.Find("Screen").GetComponent<Renderer>();
            videoPlayer.RegisterListener(this);
            _internalVideoRenderer = videoPlayer.GetVideoPlayerController().GetInternalScreen((int) KinelVideoMode.Video);
            _internalAvProRenderer = videoPlayer.GetVideoPlayerController().GetInternalScreen((int) KinelVideoMode.Stream);
            SetMirrorInversion(mirrorInverion);
            SetBackCulling(backCulling);
        }

        public override void OnKinelVideoStart() => SendCustomEventDelayedFrames(nameof(UpdateRenderer), 5);

        public override void OnKinelVideoLoop() => UpdateRenderer();

        public override void OnKinelVideoEnd() => UpdateRenderer();

        public void UpdateRenderer()
        {
            Texture texture = null;
            if (videoPlayer.IsPlaying)
            {
                if (_internalVideoRenderer == null)
                    _internalVideoRenderer = videoPlayer.GetVideoPlayerController().GetInternalScreen(((int) KinelVideoMode.Video));
            
                if(_internalAvProRenderer == null)
                    _internalAvProRenderer = videoPlayer.GetVideoPlayerController().GetInternalScreen((int) KinelVideoMode.Stream);
            
                texture = videoPlayer.GetCurrentVideoMode() == ((int) KinelVideoMode.Video)
                    ? GetTexture(_internalVideoRenderer, false)
                    : GetTexture(_internalAvProRenderer, true);
            
                if (texture == null)
                {
                    SendCustomEventDelayedFrames(nameof(UpdateRenderer), 5);
                    return;
                }

                if (videoPlayer.GetCurrentVideoMode() == (int) KinelVideoMode.Stream)
                {
                    _propertyBlock.SetInt("_IsAVPRO", 1);
                }
                else
                {
                    _propertyBlock.SetInt("_IsAVPRO", 0);
                }

                _propertyBlock.SetTexture(propertyName, texture);

            }
            
            if (texture == null)
            {
                _propertyBlock.Clear();
            }

            _screenRenderer.SetPropertyBlock(_propertyBlock, materialIndex);

        }
        
        
        public override void OnKinelVideoModeChange()
        {
#if UNITY_ANDROID
            // quest
            if (videoPlayer.GetCurrentVideoMode() == STREAM_MODE)
            {
                var localScale = _screenRenderer.transform.localScale;
                _screenRenderer.transform.localScale = new Vector3(localScale.x, -localScale.y, localScale.z);
                // var quaternion = new Quaternion();
                // quaternion.z = _screenRenderer.transform.localRotation.z + 180;
                // _screenRenderer.transform.localRotation = quaternion;
            }
            else
            {
                var localScale = _screenRenderer.transform.localScale;
                _screenRenderer.transform.localScale = new Vector3(localScale.x, -localScale.y, localScale.z);
            }
#endif
            UpdateRenderer();
        }

        private Texture GetTexture(Renderer renderer, bool avPro)
        {
            if (avPro)
            {
                return renderer.material.GetTexture(propertyName);
            }
            renderer.GetPropertyBlock(_propertyBlockInternal);
            return _propertyBlockInternal.GetTexture(propertyName);
        }

        public void SetMirrorInversion(bool active)
        {
            if (active)
            {
                _propertyBlock.SetInt("_NoMirrorInversion", 1);
            }
            else
            {
                _propertyBlock.SetInt("_NoMirrorInversion", 0);
            }
            UpdateRenderer();
        }
        
        // public void SetScreenInversion(bool active)
        // {
        //     if (active)
        //     {
        //         // _screenRenderer.transform.rotation
        //         // _screenRenderer.materials[materialIndex].SetInt("_InvertUV", 1);
        //     }
        //     else
        //     {
        //         // _screenRenderer.materials[materialIndex].SetInt("_InvertUV", 0);
        //     }
        // }

        public void SetBrightness(float darkness)
        {
#if !UNITY_ANDROID
            _propertyBlock.SetFloat("_Brightness", darkness);
#endif
            UpdateRenderer();
        }

        public void SetBackCulling(bool active)
        {
            // _propertyBlock.SetFloat("_CullMode", (active ? 0 : 2));
            _screenRenderer.materials[materialIndex].SetFloat("_CullMode", (active ? 0 : 2));           
            // UpdateRenderer();
        }
        
        

    }
}