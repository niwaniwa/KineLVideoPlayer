using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Kinel.VideoPlayer.Udon.Module
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class KinelLockModule : KinelModule
    {

        public KinelVideoPlayerUI videoPlayerUI;
        public GameObject[] toggleObjects, systemObjects;

        public void Start()
        {
            videoPlayerUI.RegisterListener(this);
        }

        public void ToggleLock()
        {
            if (!Networking.LocalPlayer.isMaster)
                return;
            
            
            var videoPlayer = videoPlayerUI.GetVideoPlayer();
            videoPlayer.TakeOwnership();
            videoPlayer.IsLock = !videoPlayer.IsLock;
            videoPlayer.RequestSerialization();
        }

        public override void OnKinelVideoPlayerLocked()
        {
            
            foreach (var target in systemObjects)
            {
                target.SetActive(!target.activeSelf);
            }
            
            if (Networking.LocalPlayer.IsOwner(videoPlayerUI.GetVideoPlayer().gameObject))
                return;
            
            foreach (var target in toggleObjects)
            {
                target.SetActive(!target.activeSelf);
            }
        }

        public override void OnKinelVideoPlayerUnlocked()
        {
            foreach (var target in systemObjects)
            {
                target.SetActive(!target.activeSelf);
            }
            
            if (Networking.LocalPlayer.IsOwner(videoPlayerUI.GetVideoPlayer().gameObject))
                return;
            
            foreach (var target in toggleObjects)
            {
                target.SetActive(!target.activeSelf);
            }
        }
        

    }
}