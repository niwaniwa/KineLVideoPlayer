using Kinel.VideoPlayer.V3.Scripts.Attribute;
using UdonSharp;
using UnityEngine;

namespace Kinel.VideoPlayer.V3.Udon.System.Component
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [KinelModuleAttribute(KinelModuleCategory.Feature, "Audio Manager", 50)]
    public class KinelAudioManager : KinelVideoListener
    {
        [SerializeField] private KinelPlayerController controller;
        [SerializeField] private AudioSource[] videoAudioSources;
        [SerializeField] private AudioSource seAudioSource;

        [SerializeField, Range(0f, 1f)] private float _masterVolume = 1f;
        private bool _isMuted = false;

        public float MasterVolume
        {
            get => _masterVolume;
            set
            {
                _masterVolume = Mathf.Clamp01(value);
                _ApplyVolumeToAll();
            }
        }

        public bool Mute
        {
            get => _isMuted;
            set
            {
                _isMuted = value;
                _ApplyMuteToAll();
            }
        }

        public void Start()
        {
            _ApplyVolumeToAll(); // Inspector で設定した既定音量を起動時に AudioSource へ反映する

            if (controller == null)
            {
                LogWarning("KinelAudioManager: controller is null");
                return;
            }

            controller.AddListener(this);
        }

        public override void OnKinelVideoSpeedChanged(float speed)
        {
            _ApplyPitch();
        }

        public override void OnKinelVideoStart()
        {
            // AVPro の reload 後などに AudioSource.pitch が戻ることがあるため再適用する
            _ApplyPitch();
        }

        public override void OnKinelMediaTypeChanged()
        {
            _ApplyPitch();
        }

        /// <summary>
        /// AVPro はネイティブで音声ピッチを変えないので、AudioSource.pitch で speed に追従させる。
        /// これをしないと低速時に音が引き伸ばされず原音ピッチのまま細切れに聞こえる。
        /// </summary>
        private void _ApplyPitch()
        {
            if (controller == null || videoAudioSources == null) return;
            bool isAvPro = controller.NowSelectedType == KinelMediaType.AvPro; // AvProだけに絞ってるがUnityVideoでも...?
            float pitch = isAvPro ? controller.NowSelectedMediaModule.GetPlaybackSpeed() : 1f;
            foreach (var src in videoAudioSources)
                if (src != null)
                    src.pitch = pitch;
        }

        // volumeScaleは呼び出し元が制御
        // 現時点でSEは動画音量に追従しない
        public void PlaySE(AudioClip clip, float volumeScale = 1f)
        {
            if (seAudioSource == null || clip == null) return;
            seAudioSource.PlayOneShot(clip, volumeScale);
        }

        private void _ApplyVolumeToAll()
        {
            if (videoAudioSources == null) return;
            foreach (var src in videoAudioSources)
                if (src != null)
                    src.volume = _masterVolume;
        }

        private void _ApplyMuteToAll()
        {
            if (videoAudioSources == null) return;
            foreach (var src in videoAudioSources)
                if (src != null)
                    src.mute = _isMuted;
        }
    }
}