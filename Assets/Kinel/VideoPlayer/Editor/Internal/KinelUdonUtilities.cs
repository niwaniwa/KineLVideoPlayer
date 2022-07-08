using System.Collections.Generic;
using UdonSharp;
using UnityEngine;

namespace Kinel.VideoPlayer.Editor.Internal
{
    /// <summary>
    /// 暫定
    /// どこかでより良い構成に変えたい
    /// </summary>
    public static class KinelUdonUtilities
    {
        public static T UdonToUserSharp<T>(UdonSharpBehaviour udon) where T : UdonSharpBehaviour
        {
            if (udon.GetType().Equals(typeof(T)))
                return udon as T;
            return null;
        }

        public static T[] GetUdonSharpComponentsByKinel<T>(this GameObject gameObject) where T: UdonSharpBehaviour
        {
            var udonSs = new List<T>();
            var udonS = gameObject.GetComponents<UdonSharpBehaviour>();

            foreach (var udon in udonS)
            {
                var udonSharp = UdonToUserSharp<T>(udon);
                if (udonSharp != null)
                    udonSs.Add(udonSharp);
            }
            
            return udonSs.ToArray();
        }
        
        public static T[] GetUdonSharpComponentsInChildrenByKinel<T>(this GameObject gameObject) where T: UdonSharpBehaviour
        {
            var udonSs = new List<T>();
            var udonS = gameObject.GetComponentsInChildren<UdonSharpBehaviour>();

            foreach (var udon in udonS)
            {
                var udonSharp = UdonToUserSharp<T>(udon);
                if (udonSharp != null)
                    udonSs.Add(udonSharp);
            }
            
            return udonSs.ToArray();
        }
    }
}