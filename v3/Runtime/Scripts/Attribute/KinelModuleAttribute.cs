using System;

namespace Kinel.VideoPlayer.V3.Scripts.Attribute
{
    /// <summary>
    /// モジュールマネージャでの列挙対象をマークするクラス属性。
    /// 派生クラスにも継承される (Inherited = true)。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class KinelModuleAttribute : System.Attribute
    {
        /// <summary>所属カテゴリ</summary>
        public KinelModuleCategory Category { get; }

        /// <summary>表示名 (null/空なら型名から自動生成)</summary>
        public string DisplayName { get; }

        /// <summary>カテゴリ内ソート順 (小さいほど上)</summary>
        public int Order { get; }

        public KinelModuleAttribute(KinelModuleCategory category, string displayName = null, int order = 100)
        {
            Category = category;
            DisplayName = displayName;
            Order = order;
        }
    }
}
