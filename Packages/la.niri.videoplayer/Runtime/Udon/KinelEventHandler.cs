
using System;
using System.Linq;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Kinel.VideoPlayer.Udon
{
    public class KinelEventHandler : UdonSharpBehaviour
    {

        private const string UDON_PREFiX = ":EventHandler:";

        private UdonSharpBehaviour[] _listeners = { };

        public void AddListener(UdonSharpBehaviour listener)
        {
            if (Array.IndexOf(_listeners, listener) != -1)
            {
                KinelDebugger.Warn($"{UDON_PREFiX} Elements are duplicated");
                return;
            }
            
            Array.Resize(ref _listeners, _listeners.Length + 1);
            _listeners[_listeners.Length - 1] = listener;
        }
        

    }

}

