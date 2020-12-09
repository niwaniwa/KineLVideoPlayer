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
            pauseObject.SetActive(kinelVideoPlayer.isPlaying);
            playObject.SetActive(!kinelVideoPlayer.isPlaying);
        }

        public void FixedUpdate()
        {
            if (isStream)
            {
                return;
            }
            if (kinelVideoPlayer.isPlaying != isPlaying)
            {
                pauseObject.SetActive(kinelVideoPlayer.isPlaying);
                playObject.SetActive(!kinelVideoPlayer.isPlaying);
                isPlaying = kinelVideoPlayer.isPlaying;
            }
        }

        public void OnPlayStateButtonClick()
        {
            if (!Networking.IsOwner(Networking.LocalPlayer, this.gameObject))
            {
                Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
            }

            //SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "SetPlay");
            
            if (isStream)
            {
                return;
            }
            
            kinelVideoPlayer.OwnerPause();
            
            if (kinelVideoPlayer.isPlaying)
            {
                //kinelVideoPlayer.isPlaying = false;
                //isPlaying = false;
                pauseObject.SetActive(isPlaying);
                playObject.SetActive(!isPlaying);
                return;
            }
            
            pauseObject.SetActive(isPlaying);
            playObject.SetActive(!isPlaying);

            //kinelVideoPlayer.isPlaying = true;
            //isPlaying = true;
        }

        public void SetPlay()
        {
            if (kinelVideoPlayer.isPlaying)
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