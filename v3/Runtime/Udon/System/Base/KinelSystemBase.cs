using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Kinel.VideoPlayer.V3.Udon.System
{
    /// <summary>
    /// System側の基底クラス
    /// </summary>
    public abstract class KinelSystemBase : UdonSharpBehaviour
    {
        protected const string DebugPrefix = "[<color=#58ACFA>KineL</color>]";

        [SerializeField] private bool debugMode = false;

        protected void Log(object message)
        {
            if (debugMode) Debug.Log($"{DebugPrefix} :{gameObject.name}:{message}");
        }

        protected void LogWarning(object message) => Debug.LogWarning($"{DebugPrefix} {message}");

        public void TakeOwnership()
        {
            Log("Take ownership (System)");
            if (Networking.IsOwner(gameObject))
                return;

            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }
    }
}