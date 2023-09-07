using System;
using Kinel.VideoPlayer.Udon.Playlist;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Kinel.VideoPlayer.Udon.Module
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class PlaylistOptionModule : UdonSharpBehaviour
    {
        [SerializeField]
        private KinelPlaylist playlist;
        [SerializeField]
        private KinelVideoPlayerUI videoPlayerUI;
        
        [SerializeField] 
        private GameObject[] togglePlayObjects, toggleStateObjects, toggleShaffleObjects;
        
        public void Start()
        {
            videoPlayerUI.GetVideoPlayer().RegisterListener(this);
        }

        public void TogglePlayState()
        {

            if (videoPlayerUI.GetVideoPlayer().IsLock &&
                !Networking.IsMaster)
                return;
            
            foreach (var obj in togglePlayObjects)
            {
                obj.SetActive(obj.activeSelf);
            }
                   
            if(!playlist.NowPlayingFlag)
                playlist.OnPlaylistSelected();
            else
                playlist.ResetLocal();
        }

        // public void ToggleLoopState()
        // {
        //     foreach (var obj in toggleStateObjects)
        //     {
        //         obj.SetActive(obj.activeSelf);
        //     }
        //     playlist.SetLoop();
        // }

        public void ToggleShuffleState()
        {
            playlist.Shuffle = !playlist.Shuffle;
            
            foreach (var obj in toggleShaffleObjects)
            {
                obj.SetActive(!obj.activeSelf);
            }
        }

        public void OnKinelVideoEnd()
        {
            if (!playlist.NowPlayingFlag)
            {
                foreach (var obj in togglePlayObjects)
                {
                    obj.SetActive(obj.activeSelf);
                }
            }
        }
        
        
        
        
        
    }
}