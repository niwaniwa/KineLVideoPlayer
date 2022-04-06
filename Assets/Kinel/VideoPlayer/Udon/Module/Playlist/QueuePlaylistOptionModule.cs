using System;
using Kinel.VideoPlayer.Udon.Playlist;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Kinel.VideoPlayer.Udon.Module
{
    public class QueuePlaylistOptionModule : UdonSharpBehaviour
    {

        [SerializeField] private KinelVideoPlayerUI videoPlayerUI;
        [SerializeField] private KinelQueuePlaylist playlist;

        [SerializeField] 
        private GameObject[] togglePlayObjects, toggleStateObjects;

        private bool usePlayStateFlag = false;
        
        public void OnEnable()
        {
            
        }

        public void TogglePlayState()
        {
            
            if (videoPlayerUI.GetVideoPlayer().IsLock &&
                Networking.IsOwner(Networking.LocalPlayer, videoPlayerUI.GetVideoPlayer().gameObject))
                return;

            foreach (var obj in togglePlayObjects)
            {
                obj.SetActive(!obj.activeSelf);
            }
            
            usePlayStateFlag = true;
            
            if(!playlist.NowPlayingFlag)
            {
                playlist.PlayQueue(0);
            }
            else
            {
                playlist.ResetPlayState();
            }
        }

        public void ToggleLoopState()
        {
            foreach (var obj in toggleStateObjects)
            {
                obj.SetActive(obj.activeSelf);
            }
            // playlist.SetLoop();
        }

        // public void OnkinelVideoStart()
        // {
        //     if (playlist.NowPlayingFlag && !usePlayStateFlag)
        //     {
        //         foreach (var obj in togglePlayObjects)
        //         {
        //             obj.SetActive(!obj.activeSelf);
        //         }
        //     }
        //         
        // }

        public void OnKinelVideoEnd()
        {
            if (!playlist.NowPlayingFlag)
            {
                foreach (var obj in togglePlayObjects)
                {
                    obj.SetActive(!obj.activeSelf);
                }

                usePlayStateFlag = false;
            }
        }

    }
}