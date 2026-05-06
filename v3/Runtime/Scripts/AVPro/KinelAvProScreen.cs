#if KINEL_AVPRO_VIDEO_ENABLED
using RenderHeads.Media.AVProVideo;
#endif
using UnityEngine;
using VRC.SDK3.Video.Components.AVPro;

namespace Kinel.VideoPlayer.V3.Scripts
{
    // 疑似的にAvProのスクリーンをMaterialに適用するスクリプト
    public class KinelAvProScreen : KinelScriptsModule
    {
#if KINEL_AVPRO_VIDEO_ENABLED
        public VRCAVProVideoScreen VideoScreen { get; set; }
        public MediaPlayer MediaPlayer { get; set; }
        private MaterialPropertyBlock _materialPropertyBlock;
        private Renderer _renderer;

        public void Start()
        {
            _materialPropertyBlock = new MaterialPropertyBlock();
            _renderer = GetComponent<Renderer>();
            MediaPlayer = GetComponent<MediaPlayer>();
        }

        public void LateUpdate()
        {
            ApplyTexture();
        }

        public void ApplyTexture()
        {
            if (!MediaPlayer || !VideoScreen) return;

            if (!MediaPlayer.Control.IsPlaying()) return;
            var textureProducer = MediaPlayer.TextureProducer;

            var texture = textureProducer.GetTexture(0);

            if (!texture) return;

            _renderer.materials[0].SetTexture(VideoScreen.TextureProperty, texture);

            var text = _renderer.materials[0].GetTexture(VideoScreen.TextureProperty);
        }
#endif
    }
}