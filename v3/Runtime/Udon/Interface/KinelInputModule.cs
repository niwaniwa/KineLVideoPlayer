using Kinel.VideoPlayer.V3.Udon.System;
using UnityEngine;
using VRC.SDK3.Components;

namespace Kinel.VideoPlayer.V3.Udon.Module
{
    public class KinelInputModule : KinelModule
    {
        
        [SerializeField] private VRCUrlInputField inputField;
        [SerializeField] private KinelLocalPlayerController controller;
        
        
        
        public void OnURLChanged()
        {
            controller.LoadUrl(inputField.GetUrl());
        }
    }
}