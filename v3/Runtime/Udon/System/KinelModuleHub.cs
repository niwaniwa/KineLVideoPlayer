using UnityEngine;

namespace Kinel.VideoPlayer.V3.Udon.System
{
    /// <summary>
    /// KineL Video Player のルート GameObject に付与する集約用のUdon
    /// エディタ拡張 (モジュールマネージャ) からの検出起点として機能する。
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Kinel/Module Hub")]
    public class KinelModuleHub : MonoBehaviour
    {
    }
}