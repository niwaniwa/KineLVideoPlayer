using Kinel.VideoPlayer.V3.Udon.System;
using UnityEngine;
using VRC.SDKBase;

namespace Kinel.VideoPlayer.V3.Udon.Interface
{
    [RequireComponent(typeof(Renderer))]
    public class KinelMeshScreen : KinelVideoListener
    {
        [SerializeField] KinelPlayerController handler;
        [SerializeField] Renderer targetRenderer;
        [SerializeField] string texturePropertyName = "_MainTex";
        [SerializeField] Texture idleTexture;

        private MaterialPropertyBlock _propertyBlock;
        private int _noMirrorInversion;

        public void OnEnable()
        {
            if (_propertyBlock == null) _propertyBlock = new MaterialPropertyBlock();
        }

        public void Start()
        {
            handler.AddListener(this);
            ApplyIdleTexture();
        }

        public override void OnKinelVideoTextureUpdated(Texture texture)
        {
            if (texture == null) return;
            ApplyTexture(texture);
        }

        public override void OnKinelVideoEnd()
        {
            ApplyIdleTexture();
        }

        public override void OnKinelMediaTypeChanged()
        {
            ApplyIdleTexture();
        }

        public override void OnKinelMediaReset()
        {
            ApplyIdleTexture();
        }

        public override void OnKinelNoMirrorInversionChanged(bool value)
        {
            _noMirrorInversion = value ? 1 : 0;
            if (!Utilities.IsValid(targetRenderer)) return;
            if (_propertyBlock == null) _propertyBlock = new MaterialPropertyBlock();
            targetRenderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetInt("_NoMirrorInversion", _noMirrorInversion);
            targetRenderer.SetPropertyBlock(_propertyBlock);
        }

        private void ApplyTexture(Texture texture)
        {
            if (!Utilities.IsValid(targetRenderer)) return;
            if (_propertyBlock == null) _propertyBlock = new MaterialPropertyBlock();

            _propertyBlock.Clear();
            targetRenderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetTexture(texturePropertyName, texture);
            _propertyBlock.SetInt("_IsAVPRO", handler.NowSelectedMediaModule.MediaType == KinelMediaType.AvPro ? 1 : 0);
            _propertyBlock.SetInt("_NoMirrorInversion", _noMirrorInversion);
            targetRenderer.SetPropertyBlock(_propertyBlock);
        }

        private void ApplyIdleTexture()
        {
            if (idleTexture == null) return;
            if (!Utilities.IsValid(targetRenderer)) return;
            if (_propertyBlock == null) _propertyBlock = new MaterialPropertyBlock();

            _propertyBlock.Clear();
            targetRenderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetTexture(texturePropertyName, idleTexture);
            _propertyBlock.SetInt("_IsAVPRO", 0);
            _propertyBlock.SetInt("_NoMirrorInversion", _noMirrorInversion);
            targetRenderer.SetPropertyBlock(_propertyBlock);
        }
    }
}