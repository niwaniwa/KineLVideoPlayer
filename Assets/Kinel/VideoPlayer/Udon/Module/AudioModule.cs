using UdonSharp;
using UnityEngine;

namespace Kinel.VideoPlayer.Udon.Module
{
    public class AudioModule : UdonSharpBehaviour
    {

        [SerializeField] public AudioSource[] sources;
        [SerializeField] public GameObject muteImage, normalImage;

        private bool mute = false;
        
        public void ToggleMute()
        {
            if (mute)
            {
                UnMute();
                mute = false;
            }
            else
            {
                Mute();
                mute = true;
            }
        }

        private void Mute()
        {
            foreach (var source in sources)
            {
                source.mute = true;
            }
            normalImage.SetActive(false);
            muteImage.SetActive(true);
        }

        private void UnMute()
        {
            foreach (var source in sources)
            {
                source.mute = false;
            }
            normalImage.SetActive(true);
            muteImage.SetActive(false);
        }

    }
}