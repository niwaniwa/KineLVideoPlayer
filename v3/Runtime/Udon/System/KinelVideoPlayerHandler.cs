using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components.Video;
using VRC.SDK3.Video.Components;
using VRC.SDK3.Video.Components.Base;
using VRC.SDKBase;
using NotImplementedException = System.NotImplementedException;

namespace Kinel.VideoPlayer.V3.Udon.System
{
    [RequireComponent(typeof(Renderer))]
    [RequireComponent(typeof(BaseVRCVideoPlayer), typeof(Renderer))]
    public class KinelVideoPlayerHandler : KinelMediaBase
    {
        private string speedAnimatorFlag = "Speed";
        private string resolutionAnimatorFlag = "Resolution";

        [SerializeField] string texturePropertyName = "_MainTex";
        [SerializeField] bool enableMipmap = true;
        [SerializeField] Material blitMaterial;
        [SerializeField] Renderer[] managedRenderers;
        [SerializeField] private Animator _animator;

        private Renderer _renderer;
        private BaseVRCVideoPlayer _videoPlayer;
        private MaterialPropertyBlock propertyBlock;
        private RenderTexture bufferedTexture;
        private Texture rawTexture;
        private Renderer[] registeredRenderers = new Renderer[0];
        private bool _isPaused;

        [FieldChangeCallback(nameof(Speed))] private float speed = 1f;

        public Texture Texture => Utilities.IsValid(bufferedTexture) ? bufferedTexture : rawTexture;
        public BaseVRCVideoPlayer VideoPlayer => _videoPlayer;

        public float Speed
        {
            get => speed;
            set
            {
                speed = Mathf.Clamp(value, 0.01f, 2f);
                Log($"Speed Changed: {speed}");
                VideoKinelVideoListener.OnKinelVideoSpeedChanged(speed);
                if (_animator != null) _animator.SetFloat(speedAnimatorFlag, speed);
            }
        }


        public void OnEnable()
        {
            _renderer = GetComponent<Renderer>();
            _videoPlayer = GetComponent<BaseVRCVideoPlayer>();
            if (propertyBlock == null) propertyBlock = new MaterialPropertyBlock();
            registeredRenderers = new Renderer[0];

            if (_animator == null)
                _animator = GetComponent<Animator>();

            if (managedRenderers != null)
                for (int i = 0; i < managedRenderers.Length; i++)
                    RegisterRenderer(managedRenderers[i]);

            if (_animator != null)
                _animator.Rebind();
        }

        public void OnDisable()
        {
            ReleaseBufferedTexture();
            registeredRenderers = new Renderer[0];
        }

        public override void LoadUrl(VRCUrl url)
        {
            if (!IsValidProtocol(url)) return;
            Debug.Log($"{DebugPrefix} Loading URL: {url}");
            _videoPlayer.LoadURL(url);
            SourceUrl = url;
            VideoKinelVideoListener.OnKinelLoadUrl(url);
        }


        public override void Play()
        {
            _videoPlayer.Play();
            _isPaused = false;
            VideoKinelVideoListener.OnKinelVideoPlay();
        }

        public override void Pause()
        {
            _videoPlayer.Pause();
            _isPaused = true;
            VideoKinelVideoListener.OnKinelVideoPause();
        }

        public override void Stop()
        {
            _videoPlayer.Stop();
            _isPaused = false;
            ReleaseBufferedTexture();
            VideoKinelVideoListener.OnKinelVideoEnd();
        }

        public override bool IsPlaying()
        {
            if (_videoPlayer == null)
            {
                return false;
            }

            return _videoPlayer.IsPlaying;
        }

        public override bool IsPaused() => _isPaused;
        public override float GetDuration() => _videoPlayer.GetDuration();
        public override float GetTime() => _videoPlayer.GetTime();
        public override float GetPlaybackSpeed() => Speed;
        public override void SetTime(float time) => _videoPlayer.SetTime(time);

        public override void SetPlaybackSpeed(float speed)
        {
            Speed = speed;
        }

        public override void SetLoop(bool loop)
        {
            _videoPlayer.Loop = loop;
        }

        public override bool GetLoop()
        {
            return _videoPlayer.Loop;
        }

        public override void SetResolution(int resolution)
        {
            this.resolution = Mathf.Clamp(resolution, 1, 8);
            Log($"Resolution Changed: {this.resolution}");
            if (_animator != null) _animator.SetInteger(resolutionAnimatorFlag, this.resolution);

            if (!IsPlaying()) return;
            ReloadMedia();
        }

        public override bool IsValidProtocol(VRCUrl url) => true;

        public override void ResetMedia()
        {
            Stop();
        }

        public override void ReloadMedia()
        {
            Stop();
            LoadUrl(SourceUrl);
        }

        public override void ResetScreen()
        {
            // ApplyTextureToTargets(Texture);
        }

        public override void AddScreen()
        {
        }

        #region Texture processing

        private void ProcessAndBroadcastTexture()
        {
            UpdateRawTexture();
            if (!Utilities.IsValid(rawTexture))
            {
                ReleaseBufferedTexture();
                return;
            }

            Texture processed = rawTexture;

            if (mediaType == KinelMediaType.AvPro && processed.GetType() == typeof(RenderTexture))
            {
                var rt = (RenderTexture)processed;
                EnsureBufferedTexture(rt);
                if (Utilities.IsValid(bufferedTexture))
                {
                    if (Utilities.IsValid(blitMaterial))
                        VRCGraphics.Blit(rt, bufferedTexture, blitMaterial);
                    else
                        VRCGraphics.Blit(rt, bufferedTexture);
                    processed = bufferedTexture;
                }
            }
            else
                ReleaseBufferedTexture();

            rawTexture = processed;
            ApplyTextureToTargets(processed);
            VideoKinelVideoListener.OnKinelVideoTextureUpdated(processed);
        }

        private void UpdateRawTexture()
        {
            if (!Utilities.IsValid(_renderer))
            {
                rawTexture = null;
                return;
            }

            if (propertyBlock == null) propertyBlock = new MaterialPropertyBlock();

            Texture _videoTexture = null;
            if (mediaType == KinelMediaType.AvPro)
            {
                _videoTexture = _renderer.material.mainTexture;
            }
            else
            {
                _renderer.GetPropertyBlock(propertyBlock);
                _videoTexture = propertyBlock.GetTexture(texturePropertyName);
            }

            rawTexture = _videoTexture;
        }

        private void EnsureBufferedTexture(RenderTexture source)
        {
            if (Utilities.IsValid(bufferedTexture) &&
                bufferedTexture.width == source.width &&
                bufferedTexture.height == source.height &&
                bufferedTexture.format == source.format)
                return;

            ReleaseBufferedTexture();
            int antiAliasing = Mathf.Max(1, source.antiAliasing);
            bufferedTexture = VRCRenderTexture.GetTemporary(source.width, source.height, 0, source.format,
                RenderTextureReadWrite.Default, antiAliasing, RenderTextureMemoryless.None, source.vrUsage, false);
            bufferedTexture.useMipMap = enableMipmap;
            bufferedTexture.autoGenerateMips = enableMipmap;
            bufferedTexture.wrapMode = TextureWrapMode.Clamp;
            bufferedTexture.filterMode = FilterMode.Bilinear;
        }

        private void ReleaseBufferedTexture()
        {
            if (!Utilities.IsValid(bufferedTexture)) return;
            VRCRenderTexture.ReleaseTemporary(bufferedTexture);
            bufferedTexture = null;
        }

        private void ApplyTextureToTargets(Texture texture)
        {
            if (registeredRenderers == null) return;
            for (int i = 0; i < registeredRenderers.Length; i++)
                ApplyTextureToRenderer(registeredRenderers[i], texture);
        }

        private void ApplyTextureToRenderer(Renderer target, Texture texture)
        {
            var outputTexture = texture;
            if (!Utilities.IsValid(target)) return;

            if (texture == null)
            {
                return;
            }

            if (propertyBlock == null) propertyBlock = new MaterialPropertyBlock();
            target.GetPropertyBlock(propertyBlock);
            propertyBlock.SetTexture(texturePropertyName, outputTexture);
            propertyBlock.SetInt("_IsAVPRO", mediaType == KinelMediaType.AvPro ? 1 : 0);
            target.SetPropertyBlock(propertyBlock);
            propertyBlock.Clear();
        }

        #region Video events

        public override void OnVideoEnd() => base.OnVideoEnd();

        public override void OnVideoLoop() => base.OnVideoLoop();

        public override void OnVideoError(VideoError videoError) => base.OnVideoError(videoError);

        public override void OnVideoStart()
        {
            base.OnVideoStart();
            retryCount = 0;
            OnVideoStartCallback();
        }

        int retryCount;

        public void OnVideoStartCallback()
        {
            UpdateRawTexture();
            if (!Utilities.IsValid(rawTexture) && retryCount < 10)
            {
                retryCount++;
                SendCustomEventDelayedFrames(nameof(OnVideoStartCallback), 10);
                return;
            }

            retryCount = 0;
            ProcessAndBroadcastTexture();
        }

        #endregion


        public void LateUpdate()
        {
            if (!_videoPlayer || !_videoPlayer.IsPlaying) return;
            ProcessAndBroadcastTexture();
        }


        public void RegisterRenderer(Renderer renderer)
        {
            if (!Utilities.IsValid(renderer)) return;
            for (int i = 0; i < registeredRenderers.Length; i++)
            {
                if (registeredRenderers[i] == renderer)
                {
                    ApplyTextureToRenderer(renderer, Texture);
                    return;
                }
            }

            Renderer[] newArray = new Renderer[registeredRenderers.Length + 1];
            registeredRenderers.CopyTo(newArray, 0);
            newArray[newArray.Length - 1] = renderer;
            registeredRenderers = newArray;
            ApplyTextureToRenderer(renderer, Texture);

            Log($"RegisterRenderer: {renderer.name}");
        }

        public void UnregisterRenderer(Renderer renderer)
        {
            if (!Utilities.IsValid(renderer) || registeredRenderers.Length == 0) return;
            int index = -1;
            for (int i = 0; i < registeredRenderers.Length; i++)
                if (registeredRenderers[i] == renderer)
                {
                    index = i;
                    break;
                }

            if (index < 0) return;

            Renderer[] newArray = new Renderer[registeredRenderers.Length - 1];
            int cursor = 0;
            for (int i = 0; i < registeredRenderers.Length; i++)
                if (i != index)
                    newArray[cursor++] = registeredRenderers[i];
            registeredRenderers = newArray;
        }

        #endregion
    }
}