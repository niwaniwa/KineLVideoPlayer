using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UdonSharp;
using UdonSharpEditor;
using UnityEngine;
using VRC.Udon.Serialization.OdinSerializer.Utilities;
using Object = System.Object;

namespace Kinel.VideoPlayer.Editor.Internal
{
    /// <summary>
    /// 暫定
    /// どこかでより良い構成に変えたい
    /// </summary>
    public static class KinelUdonUtilities
    {
        public static T[] ConvertToUdonSharpComponent<T>(UdonSharpBehaviour[] udon) where T : UdonSharpBehaviour
        {
            var udonSharpList = new List<T>();
            // udonsharp
            if(udon.Length == 0)
                return udonSharpList.ToArray();
            
            foreach (var udonSharpBehaviour in udon)
                if (udonSharpBehaviour.GetType().Equals(typeof(T)))
                    udonSharpList.Add(udonSharpBehaviour as T);
                
            

            // vanilla udon 
            //UdonSharpEditorUtility.GetProxyBehaviour(udon);
            //
            
            return udonSharpList.ToArray();
        }

        public static T[] ConvertToUdonSharpComponent<T>(UdonSharpBehaviour udon) where T : UdonSharpBehaviour
        {
            return ConvertToUdonSharpComponent<T>(new []{ udon});
        }

        public static T GetUdonSharpComponentByKinel<T>(this GameObject gameObject) where T: UdonSharpBehaviour
        {
            var udon = ConvertToUdonSharpComponent<T>(gameObject.GetComponent<UdonSharpBehaviour>());
            if (udon.Length == 0)
                return null;
            return udon[0];
        }

        public static T[] GetUdonSharpComponentsByKinel<T>(this GameObject gameObject) where T: UdonSharpBehaviour
        {
            return ConvertToUdonSharpComponent<T>(gameObject.GetComponents<UdonSharpBehaviour>());
        }
        
        
        public static T GetUdonSharpComponentInChildrenByKinel<T>(this GameObject gameObject) where T: UdonSharpBehaviour
        {
            var udon = ConvertToUdonSharpComponent<T>(gameObject.GetComponentInChildren<UdonSharpBehaviour>());
            if (udon.Length == 0)
                return null;
            return udon[0];
        }
        
        public static T[] GetUdonSharpComponentsInChildrenByKinel<T>(this GameObject gameObject) where T: UdonSharpBehaviour
        {
            return ConvertToUdonSharpComponent<T>(gameObject.GetComponentsInChildren<UdonSharpBehaviour>());
        }
        
    }
}