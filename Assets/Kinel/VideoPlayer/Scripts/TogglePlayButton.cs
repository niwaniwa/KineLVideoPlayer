using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Kinel.VideoPlayer.Scripts
{
    public class TogglePlayButton : UdonSharpBehaviour
    {

        public KinelVideoScript kinelVideoPlayer;
        public GameObject pauseObject, playObject;
        public bool isPlaying = false;
        public bool isStream;

        public void Start()
        {
            if (isStream)
            {
                return;
            }
            pauseObject.SetActive(kinelVideoPlayer.IsPlaying());
            playObject.SetActive(!kinelVideoPlayer.IsPlaying());
        }

        public void FixedUpdate()
        {
            if (isStream)
            {
                return;
            }
            if (kinelVideoPlayer.IsPlaying() != isPlaying)
            {
                pauseObject.SetActive(kinelVideoPlayer.IsPlaying());
                playObject.SetActive(!kinelVideoPlayer.IsPlaying());
                isPlaying = kinelVideoPlayer.IsPlaying();
            }
        }

        public void OnPlayStateButtonClick()
        {
            if (!Networking.IsOwner(Networking.LocalPlayer, this.gameObject))
            {
                Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
            }

            if (isStream)
            {
                return;
            }
            
            kinelVideoPlayer.OwnerPause();
            
            if (kinelVideoPlayer.IsPlaying())
            {
                pauseObject.SetActive(isPlaying);
                playObject.SetActive(!isPlaying);
                return;
            }
            
            pauseObject.SetActive(isPlaying);
            playObject.SetActive(!isPlaying);
        }

        public void SetPlay()
        {
            if (kinelVideoPlayer.IsPlaying())
            {
                kinelVideoPlayer.GetVideoPlayer().Stop();
                return;
            }
            
            if (isStream)
            {
                return;
            }
            
            kinelVideoPlayer.GetVideoPlayer().Play();
        }
        
    }
}