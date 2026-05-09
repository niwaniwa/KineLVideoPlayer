using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kinel.VideoPlayer.V3.Scripts.Attribute;
using UnityEditor;
using UnityEngine;

namespace Kinel.VideoPlayer.V3.Editor.Internal
{
    /// <summary>
    /// `KinelModuleAttribute` を持つコンポーネントをルート階層から走査するUtility
    /// </summary>
    public static class KinelModuleScanner
    {
        public readonly struct Entry
        {
            public readonly KinelModuleCategory Category;
            public readonly string DisplayName;
            public readonly int Order;
            public readonly Component Target;
            public readonly Type Type;

            public Entry(KinelModuleCategory category, string displayName, int order, Component target, Type type)
            {
                Category = category;
                DisplayName = displayName;
                Order = order;
                Target = target;
                Type = type;
            }
        }

        /// <summary>
        /// 指定したGameObject配下の全コンポーネントから`KinelModuleAttribute`を持っているものを検出する
        /// </summary>
        public static List<Entry> Scan(GameObject root)
        {
            var results = new List<Entry>();
            if (root == null) return results;

            var components = root.GetComponentsInChildren<Component>(true);
            foreach (var component in components)
            {
                if (component == null) continue;
                var type = component.GetType();
                var attr = type.GetCustomAttribute<KinelModuleAttribute>(inherit: true);
                if (attr == null) continue;

                var displayName = string.IsNullOrEmpty(attr.DisplayName)
                    ? ObjectNames.NicifyVariableName(type.Name)
                    : attr.DisplayName;

                results.Add(new Entry(attr.Category, displayName, attr.Order, component, type));
            }

            results.Sort((a, b) =>
            {
                int byCategory = ((int)a.Category).CompareTo((int)b.Category);
                if (byCategory != 0) return byCategory;
                int byOrder = a.Order.CompareTo(b.Order);
                if (byOrder != 0) return byOrder;
                return string.Compare(a.DisplayName, b.DisplayName, StringComparison.Ordinal);
            });

            return results;
        }

        /// <summary>
        /// カテゴリ単位にグルーピングした列挙。Foldout 表示用
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        public static IEnumerable<IGrouping<KinelModuleCategory, Entry>> ScanGrouped(GameObject root)
        {
            return Scan(root)
                .GroupBy(e => e.Category)
                .OrderBy(g => (int)g.Key);
        }
    }
}