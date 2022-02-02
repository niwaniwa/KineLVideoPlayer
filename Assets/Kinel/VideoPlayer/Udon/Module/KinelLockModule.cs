using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Kinel.VideoPlayer.Udon.Module
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class KinelLockModule : UdonSharpBehaviour
    {

        public KinelVideoPlayerUI videoPlayerUI;
        public GameObject[] toggleObjects;

        public void Start()
        {
            videoPlayerUI.RegisterListener(this);
        }

        public void ToggleLock()
        {
            if (!Networking.LocalPlayer.IsOwner(videoPlayerUI.GetVideoPlayer().gameObject))
                return;
            
            var videoPlayer = videoPlayerUI.GetVideoPlayer();
            videoPlayer.IsLock = !videoPlayer.IsLock;
        }

        public void OnKinelVideoPlayerLocked()
        {
            if (Networking.LocalPlayer.IsOwner(videoPlayerUI.GetVideoPlayer().gameObject))
                return;
            foreach (var target in toggleObjects)
            {
                target.SetActive(!target.activeSelf);
            }
        }

        public void OnKinelVideoPlayerUnlocked()
        {
            if (Networking.LocalPlayer.IsOwner(videoPlayerUI.GetVideoPlayer().gameObject))
                return;
            foreach (var target in toggleObjects)
            {
                target.SetActive(!target.activeSelf);
            }
        }
        

    }
}