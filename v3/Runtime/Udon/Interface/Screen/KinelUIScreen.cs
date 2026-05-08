using Kinel.VideoPlayer.V3.Udon.System;
using UnityEngine;
using UnityEngine.UI;

namespace Kinel.VideoPlayer.V3.Udon.Interface
{
    
    [RequireComponent(typeof(RawImage))]
    public class KinelUIScreen : KinelVideoListener
    {
        [SerializeField] private KinelPlayerController handler;
        [SerializeField] private Texture idleTexture;
        
        private RawImage _targetRawImage;

        public void OnEnable()
        {
            _targetRawImage = GetComponent<RawImage>();
        }

        public void Start()
        {
            handler.AddListener(this);
            ChangeIdleTexture();
        }

        public override void OnKinelVideoTextureUpdated(Texture texture)
        {
            Log("Texture updated.");
            if (texture == null)
            {
                Log("Texture is null. (UI)");
                return;
            }
            
            // TODO: AVPRO設定
            if (handler.NowSelectedMediaModule.MediaType == KinelMediaType.AvPro)
                _targetRawImage.transform.localScale = new Vector3(1, -1, 1);
            
            _targetRawImage.texture = texture;
        }

        public override void OnKinelMediaTypeChanged()
        {
            ChangeIdleTexture();
        }

        public override void OnKinelVideoEnd()
        {
            ChangeIdleTexture();
        }

        public override void OnKinelMediaReset()
        {
            ChangeIdleTexture();
        }

        private void ChangeIdleTexture()
        {
            _targetRawImage.transform.localScale = new Vector3(1, 1, 1);
            _targetRawImage.texture = idleTexture;
        }
    }

}
