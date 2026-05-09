using System.Collections.Generic;
using Kinel.VideoPlayer.V3.Editor.Internal;
using Kinel.VideoPlayer.V3.Scripts.Attribute;
using Kinel.VideoPlayer.V3.Udon.System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kinel.VideoPlayer.V3.Editor.UI
{
    /// <summary>
    /// モジュールマネージャ完全版。
    /// 左ペイン: カテゴリ Foldout でモジュール一覧を表示。
    /// 右ペイン: 選択モジュールの Inspector を `IMGUIContainer` 経由で `Editor.OnInspectorGUI()` 委譲。
    /// </summary>
    public class ModuleManagerWindow : EditorWindow
    {
        private const string UxmlPath = "Packages/la.niri.videoplayer/v3/Editor/UI/ModuleManager.uxml";
        private const string UssPath = "Packages/la.niri.videoplayer/v3/Editor/UI/ModuleManager.uss";

        // ===== State =====
        private KinelModuleHub _hub;
        private Component _currentTarget;
        private UnityEditor.Editor _currentEditor;
        private readonly List<VisualElement> _entryRows = new List<VisualElement>();

        // ===== UI elements =====
        private ObjectField _hubField;
        private VisualElement _categoriesHost;
        private Label _emptyHint;
        private Label _mainTitle;
        private VisualElement _inspectorHost;
        private IMGUIContainer _inspectorContainer;

        [MenuItem("Tools/にりらぼ(kinel)/Module Manager")]
        public static void Open()
        {
            var window = GetWindow<ModuleManagerWindow>();
            window.titleContent = new GUIContent("Module Manager");
            window.minSize = new Vector2(720, 460);
        }

        public static void Open(KinelModuleHub hub)
        {
            var window = GetWindow<ModuleManagerWindow>();
            window.titleContent = new GUIContent("Module Manager");
            window.minSize = new Vector2(720, 460);
            window.SetHub(hub);
        }

        private void CreateGUI()
        {
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UxmlPath);
            if (visualTree == null)
            {
                rootVisualElement.Add(new Label($"UXML not found: {UxmlPath}"));
                return;
            }

            visualTree.CloneTree(rootVisualElement);

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(UssPath);
            if (styleSheet != null)
                rootVisualElement.styleSheets.Add(styleSheet);

            // Cache references
            _hubField = rootVisualElement.Q<ObjectField>("hub-field");
            _categoriesHost = rootVisualElement.Q<VisualElement>("categories-host");
            _emptyHint = rootVisualElement.Q<Label>("empty-hint");
            _mainTitle = rootVisualElement.Q<Label>("main-title");
            _inspectorHost = rootVisualElement.Q<VisualElement>("inspector-host");

            _hubField.objectType = typeof(KinelModuleHub);
            _hubField.RegisterValueChangedCallback(evt => SetHub(evt.newValue as KinelModuleHub));

            rootVisualElement.Q<Button>("refresh-button").clicked += RefreshSidebar;
            rootVisualElement.Q<Button>("ping-button").clicked += () =>
            {
                if (_currentTarget != null) EditorGUIUtility.PingObject(_currentTarget.gameObject);
            };
            rootVisualElement.Q<Button>("open-inspector-button").clicked += () =>
            {
                if (_currentTarget != null) Selection.activeObject = _currentTarget.gameObject;
            };
            rootVisualElement.Q<Button>("frame-button").clicked += () =>
            {
                if (_currentTarget == null) return;
                EditorGUIUtility.PingObject(_currentTarget.gameObject);
                if (SceneView.lastActiveSceneView != null)
                {
                    Selection.activeObject = _currentTarget.gameObject;
                    SceneView.lastActiveSceneView.FrameSelected();
                }
            };

            // 右ペインの IMGUIContainer を設置
            _inspectorContainer = new IMGUIContainer(DrawInspector);
            _inspectorContainer.style.flexGrow = 1;
            _inspectorHost.Add(_inspectorContainer);

            // Hub 自動セット
            if (_hub == null)
            {
                var found = FindAnyObjectByTypeCompat();
                if (found != null) SetHub(found);
            }
            else
            {
                SetHub(_hub);
            }
        }

        private void OnDisable()
        {
            DisposeEditor();
        }

        private void SetHub(KinelModuleHub hub)
        {
            _hub = hub;
            if (_hubField != null && _hubField.value != hub)
                _hubField.SetValueWithoutNotify(hub);

            SetTarget(null);
            RefreshSidebar();
        }

        private void RefreshSidebar()
        {
            if (_categoriesHost == null) return;
            _categoriesHost.Clear();
            _entryRows.Clear();

            if (_hub == null)
            {
                if (_emptyHint != null) _emptyHint.style.display = DisplayStyle.Flex;
                return;
            }

            var grouped = KinelModuleScanner.ScanGrouped(_hub.gameObject);
            bool hasAny = false;

            foreach (var group in grouped)
            {
                hasAny = true;
                var foldout = new Foldout
                {
                    text = group.Key.ToString(),
                    value = true
                };
                foldout.AddToClassList("kinel-modulemgr-category-foldout");

                foreach (var entry in group)
                {
                    foldout.Add(BuildEntryRow(entry));
                }
                _categoriesHost.Add(foldout);
            }

            if (_emptyHint != null)
                _emptyHint.style.display = hasAny ? DisplayStyle.None : DisplayStyle.Flex;
        }

        private VisualElement BuildEntryRow(KinelModuleScanner.Entry entry)
        {
            var row = new VisualElement();
            row.AddToClassList("kinel-modulemgr-entry-row");

            var name = new Label(entry.DisplayName);
            name.AddToClassList("kinel-modulemgr-entry-name");
            row.Add(name);

            var sub = new Label(entry.Target.gameObject.name);
            sub.AddToClassList("kinel-modulemgr-entry-sub");
            row.Add(sub);

            row.RegisterCallback<MouseDownEvent>(_ => SetTarget(entry.Target));
            row.userData = entry.Target;
            _entryRows.Add(row);
            return row;
        }

        private void SetTarget(Component target)
        {
            _currentTarget = target;

            // 行ハイライト更新
            foreach (var row in _entryRows)
            {
                bool selected = row.userData == (object)target;
                if (selected) row.AddToClassList("kinel-modulemgr-entry-row--selected");
                else row.RemoveFromClassList("kinel-modulemgr-entry-row--selected");
            }

            DisposeEditor();

            if (target == null)
            {
                if (_mainTitle != null) _mainTitle.text = "モジュールを選択してください";
                _inspectorContainer?.MarkDirtyRepaint();
                return;
            }

            _currentEditor = UnityEditor.Editor.CreateEditor(target);
            if (_mainTitle != null)
            {
                var attrName = ResolveDisplayName(target);
                _mainTitle.text = $"{attrName}  ({target.gameObject.name})";
            }
            _inspectorContainer?.MarkDirtyRepaint();
        }

        private static string ResolveDisplayName(Component target)
        {
            var attr = (KinelModuleAttribute)System.Attribute.GetCustomAttribute(target.GetType(), typeof(KinelModuleAttribute), inherit: true);
            if (attr != null && !string.IsNullOrEmpty(attr.DisplayName)) return attr.DisplayName;
            return ObjectNames.NicifyVariableName(target.GetType().Name);
        }

        private void DrawInspector()
        {
            if (_currentEditor == null || _currentEditor.target == null) return;

            EditorGUI.BeginChangeCheck();
            _currentEditor.OnInspectorGUI();
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(_currentEditor.target);
                if (_currentEditor.target is Component c && c.gameObject != null && c.gameObject.scene.IsValid())
                    EditorSceneManager.MarkSceneDirty(c.gameObject.scene);
            }
        }

        private void DisposeEditor()
        {
            if (_currentEditor != null)
            {
                DestroyImmediate(_currentEditor);
                _currentEditor = null;
            }
        }

        private static KinelModuleHub FindAnyObjectByTypeCompat()
        {
#if UNITY_2023_1_OR_NEWER
            return Object.FindAnyObjectByType<KinelModuleHub>();
#else
            return Object.FindObjectOfType<KinelModuleHub>();
#endif
        }
    }
}
