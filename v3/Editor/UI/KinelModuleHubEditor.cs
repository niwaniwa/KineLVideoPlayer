using Kinel.VideoPlayer.V3.Editor.Internal;
using Kinel.VideoPlayer.V3.Scripts.Attribute;
using Kinel.VideoPlayer.V3.Udon.System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kinel.VideoPlayer.V3.Editor.UI
{
    /// <summary>
    /// `KinelModuleHub`向け
    /// カテゴリごとに Foldout を並べ、各モジュール行に `Ping` / `Inspect` ボタンを置く。
    /// 編集 UI は出さずに`ModuleManagerWindow`へ流す
    /// </summary>
    [CustomEditor(typeof(KinelModuleHub))]
    public class KinelModuleHubEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            root.style.paddingTop = 4;

            var hub = (KinelModuleHub)target;

            // 上部: Module Manager ウィンドウ起動ボタン
            var openButton = new Button(() => ModuleManagerWindow.Open(hub))
            {
                text = "Open in Module Manager"
            };
            openButton.style.marginBottom = 6;
            root.Add(openButton);

            BuildCategorySections(root, hub);

            return root;
        }

        private static void BuildCategorySections(VisualElement parent, KinelModuleHub hub)
        {
            var grouped = KinelModuleScanner.ScanGrouped(hub.gameObject);
            bool hasAny = false;

            foreach (var group in grouped)
            {
                hasAny = true;
                var foldout = new Foldout
                {
                    text = group.Key.ToString(),
                    value = true
                };
                foldout.style.marginBottom = 2;

                foreach (var entry in group)
                {
                    foldout.Add(BuildEntryRow(entry));
                }

                parent.Add(foldout);
            }

            if (!hasAny)
            {
                var hint = new HelpBox(
                    "KinelModule 属性付きのコンポーネントが子階層に見つかりませんでした。",
                    HelpBoxMessageType.Info);
                parent.Add(hint);
            }
        }

        private static VisualElement BuildEntryRow(KinelModuleScanner.Entry entry)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.marginLeft = 4;
            row.style.marginRight = 4;
            row.style.marginTop = 1;
            row.style.marginBottom = 1;

            var label = new Label(entry.DisplayName);
            label.style.flexGrow = 1;
            label.style.unityTextAlign = TextAnchor.MiddleLeft;
            row.Add(label);

            // GameObject 名サブテキスト
            var sub = new Label($"({entry.Target.gameObject.name})");
            sub.style.opacity = 0.6f;
            sub.style.marginRight = 4;
            sub.style.unityTextAlign = TextAnchor.MiddleRight;
            row.Add(sub);

            var pingButton = new Button(() =>
            {
                if (entry.Target != null) EditorGUIUtility.PingObject(entry.Target.gameObject);
            }) { text = "Ping" };
            pingButton.style.minWidth = 44;
            row.Add(pingButton);

            var inspectButton = new Button(() =>
            {
                if (entry.Target != null) Selection.activeObject = entry.Target.gameObject;
            }) { text = "Inspect" };
            inspectButton.style.minWidth = 56;
            row.Add(inspectButton);

            return row;
        }
    }
}