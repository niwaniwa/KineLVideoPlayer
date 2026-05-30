using Kinel.VideoPlayer.V3.Udon.System;
using VRC.SDKBase;

namespace Kinel.VideoPlayer.V3.Udon.System.Component
{
    /// <summary>
    /// 操作権限の判定を委譲するための抽象基底
    /// 派生クラスを差し替えることで権限モデルを変更できる
    /// CanOperateは全クライアントで同じ結果を返す純粋関数で実装する
    /// - OnOwnershipRequestがrequesterとowner双方ローカルで評価されるため
    /// TODO: ロールベースの制御など
    /// </summary>
    public abstract class KinelPermissionProviderBase : KinelSystemBase
    {
        public virtual bool CanOperate(VRCPlayerApi player)
        {
            return player != null && player.IsValid() && player.isMaster;
        }
    }
}