using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Kinel.VideoPlayer.V3.Udon.System.Component
{
    /// <summary>
    /// Master または InstanceOwner を権限者とみなすProvider
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class KinelMasterPermissionProvider : KinelPermissionProviderBase
    {
        [SerializeField] private bool allowInstanceOwner = true;

        public override bool CanOperate(VRCPlayerApi player)
        {
            if (player == null || !player.IsValid()) return false;
            return player.isMaster || (allowInstanceOwner && player.isInstanceOwner);
        }
    }
}