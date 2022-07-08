using UdonSharp;
using UnityEngine;

namespace Kinel.VideoPlayer.Udon.Module
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class KinelScreenModule : UdonSharpBehaviour
    {

        
        [SerializeField] public KinelVideoPlayer videoPlayer;
        
        [SerializeField] public string propertyName;
        [SerializeField] public string screenName;
        [SerializeField] public int materialIndex;

        [SerializeField] public bool mirrorInverion;
        [SerializeField] public bool backCulling;
        [SerializeField] public float transparency;

        public bool isAutoFill = false;

        private MaterialPropertyBlock _propertyBlock, _propertyBlockInternal;
        private Renderer _screenRenderer, _internalVideoRenderer, _internalAvProRenderer;

        private const int VIDEO_MODE = 0;
        private const int STREAM_MODE = 1;

        private Texture _mainTex;
        
        public void Start()
        {
            _propertyBlock = new MaterialPropertyBlock();
            _propertyBlockInternal = new MaterialPropertyBlock();
            _screenRenderer = gameObject.transform.Find("Screen").GetComponent<Renderer>();
            videoPlayer.RegisterListener(this);
            _internalVideoRenderer = videoPlayer.GetVideoPlayerController().GetInternalScreen(VIDEO_MODE);
            _internalAvProRenderer = videoPlayer.GetVideoPlayerController().GetInternalScreen(STREAM_MODE);
            SetMirrorInversion(mirrorInverion);
            SetBackCulling(backCulling);
        }

        public void OnKinelVideoStart()
        {
            SendCustomEventDelayedFrames(nameof(UpdateRenderer), 5);
        }

        public void OnKinelVideoLoop()
        {
            UpdateRenderer();
        }

        public void UpdateRenderer()
        {
            Debug.Log("UPDATE RENDERER");
            Texture texture = null;

            if (videoPlayer.IsPlaying)
            {
                
                if (_internalVideoRenderer == null)
                    _internalVideoRenderer = videoPlayer.GetVideoPlayerController().GetInternalScreen(VIDEO_MODE);
            
                if(_internalAvProRenderer == null)
                    _internalAvProRenderer = videoPlayer.GetVideoPlayerController().GetInternalScreen(STREAM_MODE);
            
                texture = videoPlayer.GetCurrentVideoMode() == VIDEO_MODE
                    ? GetTexture(_internalVideoRenderer, false)
                    : GetTexture(_internalAvProRenderer, true);
            
                if (texture == null)
                {
                    SendCustomEventDelayedFrames(nameof(UpdateRenderer), 5);
                    return;
                }
                
                // AVPro向けのシェーダー設定が動作しない

                if (videoPlayer.GetCurrentVideoMode() == STREAM_MODE)
                {
                    // _screenRenderer.material.EnableKeyword("IS_AVPRO");
                    _propertyBlock.SetInt("_IsAVPRO", 1);
                }
                else
                {
                    // _screenRenderer.material.DisableKeyword("IS_AVPRO");
                    _propertyBlock.SetInt("_IsAVPRO", 0);
                }


                _propertyBlock.SetTexture(propertyName, texture);

               
 
                
            }

            // if (texture == null)
            // {
            //     _propertyBlock.Clear();
            //     // _screenRenderer.set
            // }

            _screenRenderer.SetPropertyBlock(_propertyBlock, materialIndex);

            // videoPlayer.GetVideoPlayerController().GetUnityVideoPlayer().
            
        }
        
        public void OnKinelVideoModeChange()
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