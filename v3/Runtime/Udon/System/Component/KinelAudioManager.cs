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

        private float _masterVolume = 1f;
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
            if (controller == null)
            {
                LogWarning("KinelAudioManager: controller is null");
                return;
            }

            controller.AddListener(this);
        }

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