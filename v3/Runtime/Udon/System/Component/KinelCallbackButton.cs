using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;

namespace Kinel.VideoPlayer.V3.Udon.System.Component
{
    [RequireComponent(typeof(Button))]
    public class KinelCallbackButton : UdonSharpBehaviour
    {
        private Button _button;
        
        public void Start()
        {
        }
        
        public void OnClick()
        {
        }
        
    }
}