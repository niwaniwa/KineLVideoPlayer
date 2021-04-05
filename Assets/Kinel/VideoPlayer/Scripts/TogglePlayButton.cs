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
        
        private bool isPlaying = false;
        private const int VIDEO_MODE = 0;
        private const int STREAM_MODE = 1;

        public void Start()
        {
            if (kinelVideoPlayer.GetPlayMode() == STREAM_MODE)
                return;
            
            pauseObject.SetActive(kinelVideoPlayer.IsPlaying());
            playObject.SetActive(!kinelVideoPlayer.IsPlaying());
        }

        public void FixedUpdate()
        {
            if (kinelVideoPlayer.GetPlayMode() == STREAM_MODE)
                return;
            
            if (kinelVideoPlayer.IsPlaying() != isPlaying)
            {
                pauseObject.SetActive(kinelVideoPlayer.IsPlaying());
                playObject.SetActive(!kinelVideoPlayer.IsPlaying());
                isPlaying = kinelVideoPlayer.IsPlaying();
            }
        }

        public void OnPlayStateButtonClick()
        {
            if (kinelVideoPlayer.GetPlayMode() == STREAM_MODE || (kinelVideoPlayer.masterOnly && !Networking.LocalPlayer.isMaster) )
                return;

            if (!kinelVideoPlayer.IsPlaying() && !kinelVideoPlayer.IsPause())
                return;
            
            if (!Networking.IsOwner(Networking.LocalPlayer, kinelVideoPlayer.gameObject))
                Networking.SetOwner(Networking.LocalPlayer, kinelVideoPlayer.gameObject);
            
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

    }
}