using HoshinoLabs.IwaSync3.Udon;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components.Video;
using VRC.SDKBase;

namespace Kinel.VideoPlayer.V3.Udon.Module
{
    public class KinelIwasyncScreen : VideoCoreEventListener
    {
        protected string DebugPrefix = $"[<color=#58ACFA>KineL</color>/<color=#47F1FF>#{IwaSync3.APP_NAME}</color>]";

        [Header("Main")] [SerializeField] VideoCore core;
        [Header("kinel")] [SerializeField] private KinelUIController controller;

        // [Header("Options")] [SerializeField] int materialIndex = 0;
        [SerializeField] string textureProperty = "_MainTex";
        [SerializeField] bool idleScreenOff = false;
        [SerializeField] Texture idleScreenTexture = null;
        [SerializeField] float aspectRatio = 1.777778f;

        [SerializeField, FieldChangeCallback(nameof(mirror))]
        bool defaultMirror = true;

        [SerializeField, Range(0f, 5f), FieldChangeCallback(nameof(emissiveBoost))]
        float defaultEmissiveBoost = 1f;

        [SerializeField] Renderer screen;

        MaterialPropertyBlock _properties;

        private void Start()
        {
            Log($" Started `{nameof(VideoScreen)}`.");

            // core.AddListener(this);

            _properties = new MaterialPropertyBlock();
        }

        #region RoomEvent

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            UpdateView();
        }

        #endregion

        #region VideoEvent

        public override void OnVideoEnd()
        {
            UpdateView();
        }

        public override void OnVideoError(VideoError videoError)
        {
            UpdateView();
        }

        public override void OnVideoStart()
        {
            UpdateView();
        }

        #endregion

        #region VideoCoreEvent

        public override void OnPlayerPlay()
        {
            UpdateView();
        }

        public override void OnPlayerPause()
        {
            UpdateView();
        }

        public override void OnPlayerStop()
        {
            UpdateView();
        }

        public override void OnChangeBrightness()
        {
            UpdateView();
        }

        #endregion

        public void UpdateView()
        {
            if (!controller.IsIwasync) return;
            screen.enabled = !idleScreenOff || core.isPlaying;
            var texture = idleScreenTexture;
            var aspect = aspectRatio;
            if (texture != null)
            {
                aspect = (float)texture.width / texture.height;
            }

            if (core.isPlaying)
            {
                if (core.texture != null)
                {
                    // SendCustomEventDelayedFrames(nameof(UpdateView), 0);
                    texture = core.texture;
                    // else
                    // {
                    //     texture = core.texture;
                }
            }

            _properties.Clear();
            if (texture != null)
            {
                _properties.SetTexture(textureProperty, texture);
            }

            _properties.SetInt("_IsAVProVideo", core.isPlaying ? (core.isModeVideo ? 0 : 1) : 0);
            screen.SetPropertyBlock(_properties);
        }

        private void Update()
        {
            if (!controller.IsIwasync) return;
            if (!core.isPlaying)
                return;

            var texture = core.texture;
            if (texture != null)
            {
                _properties.SetTexture(textureProperty, texture);
                screen.SetPropertyBlock(_properties);
            }
        }

        public bool mirror
        {
            get => defaultMirror;
            set
            {
                defaultMirror = value;
                UpdateMirror();
            }
        }

        void UpdateMirror()
        {
            UpdateView();
        }

        public float emissiveBoost
        {
            get => defaultEmissiveBoost;
            set
            {
                defaultEmissiveBoost = Mathf.Clamp(value, 0f, 5f);
                UpdateEmissiveBoost();
            }
        }

        void UpdateEmissiveBoost()
        {
            UpdateView();
        }

        /// <summary>
        /// iwasync をOFFにする瞬間に呼んで、スクリーンに残った動画テクスチャ参照を解除する
        /// （Renderer.enabled は触らず、MainTexをidleに差し替える）
        /// </summary>
        public void ResetScreen()
        {
            if (!Utilities.IsValid(screen)) return;
            if (_properties == null) _properties = new MaterialPropertyBlock();

            Log($"ResetScreen.");
            _properties.Clear();
            _properties.SetTexture(textureProperty, Texture2D.blackTexture);

            _properties.SetInt("_IsAVProVideo", 0);
            screen.SetPropertyBlock(_properties);
        }

        private void Log(object message) => Debug.Log($"{DebugPrefix} {message}");
    }
}