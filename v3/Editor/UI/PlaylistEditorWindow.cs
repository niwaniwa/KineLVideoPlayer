using System.Collections.Generic;
using Kinel.VideoPlayer.V3.Scripts;
using Kinel.VideoPlayer.V3.Scripts.VideoPlayer;
using Kinel.VideoPlayer.V3.Udon.System;
using Kinel.VideoPlayer.V3.Udon.System.Component;
using UdonSharpEditor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDKBase;

namespace Kinel.VideoPlayer.V3.Editor.UI
{
    public class PlaylistEditorWindow : EditorWindow
    {
        private const string UxmlPath = "Packages/la.niri.videoplayer/v3/Editor/UI/PlaylistEditor.uxml";
        private const string UssPath = "Packages/la.niri.videoplayer/v3/Editor/UI/PlaylistEditor.uss";

        // ===== State =====
        private KinelPlaylistScript _behaviour;
        private SerializedObject _serializedObject;
        private SerializedObject _selectedItemSO;
        private SerializedProperty _tracksProperty;
        private bool _isDirty;

        // Open(KinelPlaylistScript) で渡された target を保持して CreateGUI 完了後に適用する
        private KinelPlaylistScript _pendingTarget;

        // ===== UI elements =====
        private ObjectField _behaviourField;
        private ListView _playlistList;
        private ListView _trackList;
        private VisualElement _mainHeader;
        private TextField _playlistNameField;
        private Label _playlistMetaLabel;
        private Label _emptyHint;
        private TextField _importUrlField;
        private Label _dirtyIndicator;

        [MenuItem("Tools/にりらぼ(kinel)/Playlist Editor")]
        public static void Open()
        {
            var window = GetWindow<PlaylistEditorWindow>();
            window.titleContent = new GUIContent("Playlist Editor");
            window.minSize = new Vector2(700, 440);
        }

        /// <summary>
        /// 指定 behaviour を事前選択した状態で Editor を開く。Inspector からの遷移ボタン向け。
        /// </summary>
        public static void Open(KinelPlaylistScript behaviour)
        {
            var window = GetWindow<PlaylistEditorWindow>();
            window.titleContent = new GUIContent("Playlist Editor");
            window.minSize = new Vector2(700, 440);
            window._pendingTarget = behaviour;
            window.ApplyPendingTarget();
        }

        private void ApplyPendingTarget()
        {
            // _behaviourField が null の場合は CreateGUI 完了前。CreateGUI の末尾で再度呼ばれる。
            if (_pendingTarget == null || _behaviourField == null) return;
            _behaviourField.value = _pendingTarget;   // RegisterValueChangedCallback 経由で SetBehaviour が走る
            _pendingTarget = null;
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
            _behaviourField = rootVisualElement.Q<ObjectField>("behaviour-field");
            _playlistList = rootVisualElement.Q<ListView>("playlist-list");
            _trackList = rootVisualElement.Q<ListView>("track-list");
            _mainHeader = rootVisualElement.Q<VisualElement>("main-header");
            _playlistNameField = rootVisualElement.Q<TextField>("playlist-name");
            _playlistMetaLabel = rootVisualElement.Q<Label>("playlist-meta");
            _emptyHint = rootVisualElement.Q<Label>("empty-hint");
            _importUrlField = rootVisualElement.Q<TextField>("import-url");
            _dirtyIndicator = rootVisualElement.Q<Label>("dirty-indicator");

            _behaviourField.objectType = typeof(KinelPlaylistScript);
            _behaviourField.RegisterValueChangedCallback(evt => SetBehaviour(evt.newValue as KinelPlaylistScript));

            rootVisualElement.Q<Button>("save-button").clicked += OnSaveClicked;
            rootVisualElement.Q<Button>("reverse-button").clicked += OnReverseClicked;
            rootVisualElement.Q<Button>("clear-button").clicked += OnClearClicked;
            rootVisualElement.Q<Button>("setall-avpro-button").clicked += OnSetAllAvProClicked;
            rootVisualElement.Q<Button>("import-button").clicked += OnImportClicked;

            // ListView 既定の + ボタンは InsertArrayElementAtIndex 経由で動き、
            // [Serializable] クラスの List に対しては新要素が前要素と C# 参照を共有する。
            // 既定経路をバイパスし、Import と同じ「item.Tracks.Add(new ...) + Rebuild」パターンで置換する。
            var addButton = _trackList.Q<Button>("unity-list-view__add-button");
            if (addButton != null) addButton.clickable = new Clickable(OnTrackAddClicked);

            // CreateGUI は一度しか呼ばれないので += で OK
            _trackList.itemsRemoved += OnTrackListChanged;

            rootVisualElement.RegisterCallback<SerializedPropertyChangeEvent>(_ => MarkDirty());

            _playlistNameField.RegisterValueChangedCallback(evt =>
            {
                if (_behaviour == null || _behaviour.playlists == null || _playlistList == null) return;
                var idx = _playlistList.selectedIndex;
                if (idx < 0 || idx >= _behaviour.playlists.Count) return;
                var item = _behaviour.playlists[idx];
                if (item != null) item.name = evt.newValue ?? string.Empty;
                _playlistList.RefreshItem(idx);
            });

            ShowMain(false);
            UpdateDirtyIndicator();

            // Open(KinelPlaylistScript) で渡されていた pending target をここで適用する
            ApplyPendingTarget();
        }

        // ============================================================
        //  Behaviour selection
        // ============================================================
        private void SetBehaviour(KinelPlaylistScript behaviour)
        {
            _behaviour = behaviour;
            if (_behaviour == null)
            {
                _serializedObject = null;
                _playlistList.itemsSource = null;
                _playlistList.Rebuild();
                ShowMain(false);
                return;
            }

            if (_behaviour.playlists == null)
            {
                _behaviour.playlists = new List<KinelPlaylistItem>();
                EditorUtility.SetDirty(_behaviour);
            }

            _serializedObject = new SerializedObject(_behaviour);
            BindPlaylistList();
            ShowMain(false);
            SetDirty(false);
        }

        // ============================================================
        //  Playlist sidebar
        // ============================================================
        private void BindPlaylistList()
        {
            _playlistList.itemsSource = _behaviour.playlists;
            _playlistList.makeItem = MakePlaylistAvatarRow;
            _playlistList.bindItem = BindPlaylistAvatarRow;

            _playlistList.itemsAdded += OnPlaylistItemsAdded;
            _playlistList.itemsRemoved += OnPlaylistItemsRemoved;
            _playlistList.itemIndexChanged += OnPlaylistItemReordered;
            _playlistList.selectedIndicesChanged += OnPlaylistSelectionChanged;

            _playlistList.Rebuild();
        }

        private VisualElement MakePlaylistAvatarRow()
        {
            var row = new VisualElement();
            row.AddToClassList("kinel-avatar-row");

            var avatar = new VisualElement();
            avatar.AddToClassList("kinel-avatar");

            var letter = new Label();
            letter.AddToClassList("kinel-avatar-letter");
            avatar.Add(letter);

            row.Add(avatar);
            return row;
        }

        private void BindPlaylistAvatarRow(VisualElement element, int index)
        {
            if (_behaviour == null || index < 0 || index >= _behaviour.playlists.Count) return;
            var item = _behaviour.playlists[index];
            var letter = element.Q<Label>(className: "kinel-avatar-letter");
            if (letter == null) return;

            var playlistName = item != null ? item.playlistName : null;
            var hasName = playlistName != null && playlistName.Length > 0;
            letter.text = hasName ? char.ToUpperInvariant(playlistName[0]).ToString() : "?";
            element.tooltip = hasName ? playlistName : "(unnamed)";
        }

        private void OnPlaylistItemsAdded(IEnumerable<int> indices)
        {
            if (_behaviour == null) return;

            foreach (var index in indices)
            {
                if (index < 0 || index >= _behaviour.playlists.Count) continue;

                var go = new GameObject($"Playlist {_behaviour.playlists.Count}");
                go.transform.SetParent(_behaviour.transform);
                var item = go.AddComponent<KinelPlaylistItem>();
                item.playlistName = go.name;
                _behaviour.playlists[index] = item;
            }

            EditorUtility.SetDirty(_behaviour);
            _playlistList.Rebuild();
            MarkDirty();
        }

        private void OnPlaylistItemsRemoved(IEnumerable<int> indices)
        {
            if (_behaviour == null) return;

            foreach (var index in indices)
            {
                if (index < 0 || index >= _behaviour.playlists.Count) continue;
                var item = _behaviour.playlists[index];
                if (item != null)
                    DestroyImmediate(item.gameObject);
            }

            EditorUtility.SetDirty(_behaviour);
            ShowMain(false);
            MarkDirty();
        }

        private void OnPlaylistItemReordered(int srcIndex, int dstIndex)
        {
            if (_behaviour == null) return;

            for (int i = 0; i < _behaviour.playlists.Count; i++)
            {
                if (_behaviour.playlists[i] != null)
                    _behaviour.playlists[i].transform.SetSiblingIndex(i);
            }

            EditorUtility.SetDirty(_behaviour);
            MarkDirty();
        }

        private void OnPlaylistSelectionChanged(IEnumerable<int> indices)
        {
            int selected = -1;
            foreach (var i in indices)
            {
                selected = i;
                break;
            }

            if (_behaviour == null || selected < 0 || selected >= _behaviour.playlists.Count)
            {
                ShowMain(false);
                return;
            }

            var playlistItem = _behaviour.playlists[selected];
            if (playlistItem == null)
            {
                ShowMain(false);
                return;
            }

            BindMain(playlistItem);
        }

        // ============================================================
        //  Main panel (header + track list)
        // ============================================================
        private void ShowMain(bool show)
        {
            _mainHeader.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
            _trackList.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
            _emptyHint.style.display = show ? DisplayStyle.None : DisplayStyle.Flex;
            rootVisualElement.Q<VisualElement>("actions").style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void BindMain(KinelPlaylistItem playlistItem)
        {
            _trackList.Unbind();
            _selectedItemSO = new SerializedObject(playlistItem);
            _tracksProperty = _selectedItemSO.FindProperty(nameof(KinelPlaylistItem.Tracks));

            var nameProp = _selectedItemSO.FindProperty(nameof(KinelPlaylistItem.playlistName));
            _playlistNameField.BindProperty(nameProp);
            UpdateMetaLabel(playlistItem);

            _trackList.makeItem = MakeTrackRow;
            _trackList.bindItem = BindTrackRow;
            _trackList.unbindItem = UnbindTrackRow;
            _trackList.bindingPath = nameof(KinelPlaylistItem.Tracks);
            _trackList.Bind(_selectedItemSO);

            ShowMain(true);
        }

        private void UpdateMetaLabel(KinelPlaylistItem item)
        {
            if (item == null || item.Tracks == null || item.Tracks.Count == 0)
            {
                _playlistMetaLabel.text = "(empty)";
                return;
            }

            var counts = new Dictionary<KinelMediaType, int>();
            foreach (var t in item.Tracks)
            {
                if (t == null) continue;
                if (!counts.ContainsKey(t.Type)) counts[t.Type] = 0;
                counts[t.Type]++;
            }

            var parts = new List<string>();
            foreach (var kvp in counts) parts.Add($"{kvp.Key} {kvp.Value}");
            _playlistMetaLabel.text = $"{item.Tracks.Count} tracks {string.Join(", ", parts)}";
        }

        // ============================================================
        //  Track row (custom layout: Type chip + Title + URL)
        // ============================================================
        private VisualElement MakeTrackRow()
        {
            // BindableElement にしないと bindItem 内の Bind(SO) で SerializedObjectBindEvent が伝播せず、
            // 子の相対 bindingPath が解決されない。
            var row = new BindableElement();
            row.AddToClassList("kinel-track-row");

            var header = new VisualElement();
            header.AddToClassList("kinel-track-header");

            var typeField = new EnumField(KinelMediaType.AvPro);
            typeField.AddToClassList("kinel-type-chip");
            typeField.name = "track-type";
            typeField.bindingPath = nameof(KinelMediaTrackImpl.Type);
            header.Add(typeField);

            row.Add(header);

            var fields = new VisualElement();
            fields.AddToClassList("kinel-track-fields");

            var titleField = new TextField("Title") { name = "track-title" };
            titleField.bindingPath = nameof(KinelMediaTrackImpl.Title);
            fields.Add(titleField);

            // URL は VRCUrl 内部 string への nested path (Url.m_Url など) になる。
            // ListView 行コンテキスト経由の 2 段ドット bindingPath は安定して解決されないため
            // BindTrackRow で直接 BindProperty する。
            var urlField = new TextField("URL") { name = "track-url" };
            fields.Add(urlField);

            row.Add(fields);

            typeField.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue is KinelMediaType v)
                {
                    ApplyTypeChipClass(typeField, v);
                    MarkDirty();
                }
            });

            return row;
        }

        private void BindTrackRow(VisualElement element, int index)
        {
            if (!(element is BindableElement bindable)) return;
            if (_tracksProperty == null || index < 0 || index >= _tracksProperty.arraySize) return;

            // SerializedProperty は iterator なので Copy() で独立 handle にしないと、
            // bindItem が次 index で呼ばれた際に internal state が mutate され、
            // 過去 row の binding も同じ末尾 path を指してしまう (= リアルタイム mirror の原因)。
            var elemProp = _tracksProperty.GetArrayElementAtIndex(index).Copy();
            bindable.Unbind();
            bindable.bindingPath = elemProp.propertyPath;
            bindable.Bind(elemProp.serializedObject);

            var urlField = element.Q<TextField>("track-url");
            if (urlField != null)
            {
                var urlProp = elemProp.FindPropertyRelative(nameof(KinelMediaTrackImpl.Url))?.Copy();
                var urlStringSrc = urlProp?.FindPropertyRelative("m_Url") ?? urlProp?.FindPropertyRelative("url");
                var urlStringProp = urlStringSrc?.Copy();
                urlField.Unbind();
                if (urlStringProp != null) urlField.BindProperty(urlStringProp);
            }

            // binding 反映は SetValueWithoutNotify 経由なので ChangeEvent が発火しない場合がある。
            var typeField = element.Q<EnumField>("track-type");
            if (typeField != null) ApplyTypeChipClass(typeField, (KinelMediaType)typeField.value);
        }

        private void UnbindTrackRow(VisualElement element, int index)
        {
            if (element is BindableElement bindable) bindable.Unbind();
            // urlField は別 BindProperty を持つので明示的に Unbind する
            var urlField = element.Q<TextField>("track-url");
            urlField?.Unbind();
        }

        private static void ApplyTypeChipClass(VisualElement typeField, KinelMediaType type)
        {
            typeField.RemoveFromClassList("kinel-type-chip--avpro");
            typeField.RemoveFromClassList("kinel-type-chip--unity");
            typeField.RemoveFromClassList("kinel-type-chip--image");

            switch (type)
            {
                case KinelMediaType.AvPro: typeField.AddToClassList("kinel-type-chip--avpro"); break;
                case KinelMediaType.UnityVideo: typeField.AddToClassList("kinel-type-chip--unity"); break;
                case KinelMediaType.Image: typeField.AddToClassList("kinel-type-chip--image"); break;
            }
        }

        #region action

        private void OnReverseClicked()
        {
            var item = GetSelectedPlaylistItem();
            if (item == null) return;

            item.Tracks.Reverse();
            EditorUtility.SetDirty(item);
            _selectedItemSO?.Update();
            _trackList.Rebuild();
            UpdateMetaLabel(item);
            MarkDirty();
        }

        private void OnClearClicked()
        {
            var item = GetSelectedPlaylistItem();
            if (item == null) return;

            item.Tracks.Clear();
            EditorUtility.SetDirty(item);
            _selectedItemSO?.Update();
            _trackList.Rebuild();
            UpdateMetaLabel(item);
            MarkDirty();
        }

        private void OnSetAllAvProClicked()
        {
            var item = GetSelectedPlaylistItem();
            if (item == null || item.Tracks == null || item.Tracks.Count == 0) return;

            foreach (var track in item.Tracks)
            {
                if (track == null) continue;
                track.Type = KinelMediaType.AvPro;
            }

            EditorUtility.SetDirty(item);
            _selectedItemSO?.Update();
            _trackList.Rebuild();
            UpdateMetaLabel(item);
            MarkDirty();
        }

        private void OnImportClicked()
        {
            var item = GetSelectedPlaylistItem();
            if (item == null) return;

            var url = _importUrlField.value;
            if (string.IsNullOrEmpty(url)) return;

            var data = PlaylistDataImporter.GetYoutubePlaylist(url);
            if (data?.videos == null) return;

            foreach (var v in data.videos)
                item.Tracks.Add(new KinelMediaTrackImpl(new VRCUrl(v.url), v.title, KinelMediaType.AvPro));

            EditorUtility.SetDirty(item);
            _selectedItemSO?.Update();
            _trackList.Rebuild();
            UpdateMetaLabel(item);
            MarkDirty();
        }

        private void OnSaveClicked()
        {
            if (_behaviour == null) return;

            var udon = _behaviour.GetComponent<KinelPlaylist>();
            if (udon == null)
            {
                Debug.LogError("KinelPlaylist component が見つかりません。");
                return;
            }

            _selectedItemSO?.ApplyModifiedProperties();
            _serializedObject?.ApplyModifiedProperties();

            SavePlaylistToUdon(_behaviour, udon);

            EditorUtility.SetDirty(udon);
            EditorSceneManager.MarkSceneDirty(udon.gameObject.scene);
            EditorSceneManager.SaveScene(udon.gameObject.scene);

            SetDirty(false);
            Debug.Log("Playlist を Udon に保存しました。");
        }

        private void OnTrackAddClicked()
        {
            var item = GetSelectedPlaylistItem();
            if (item == null) return;
            item.Tracks.Add(new KinelMediaTrackImpl());
            EditorUtility.SetDirty(item);
            _selectedItemSO?.Update();
            // array resize 後は cached SerializedProperty handle が invalid 化しうるので再取得する
            _tracksProperty = _selectedItemSO?.FindProperty(nameof(KinelPlaylistItem.Tracks));
            _trackList.Rebuild();
            UpdateMetaLabel(item);
            MarkDirty();
        }

        private void OnTrackListChanged(IEnumerable<int> _)
        {
            _selectedItemSO?.ApplyModifiedProperties();
            _selectedItemSO?.Update();
            // array resize 後は cached handle が invalid 化、かつ残行の binding は stale 化するため
            // 再取得 + Rebuild で stale handle を完全に破棄して再構築する。
            _tracksProperty = _selectedItemSO?.FindProperty(nameof(KinelPlaylistItem.Tracks));
            _trackList.Rebuild();
            var item = GetSelectedPlaylistItem();
            if (item != null) UpdateMetaLabel(item);
            MarkDirty();
        }

        private KinelPlaylistItem GetSelectedPlaylistItem()
        {
            if (_behaviour == null) return null;
            var index = _playlistList.selectedIndex;
            if (index < 0 || index >= _behaviour.playlists.Count) return null;
            return _behaviour.playlists[index];
        }

        #endregion

        #region dirtytracking

        private void MarkDirty() => SetDirty(true);

        private void SetDirty(bool dirty)
        {
            if (_isDirty == dirty) return;
            _isDirty = dirty;
            UpdateDirtyIndicator();
        }

        private void UpdateDirtyIndicator()
        {
            if (_dirtyIndicator == null) return;
            _dirtyIndicator.style.display = _isDirty ? DisplayStyle.Flex : DisplayStyle.None;
        }

        #endregion

        #region savelogic

        /// <summary>
        /// Udonにデータを保存する
        /// </summary>
        /// <param name="behaviour">target playlist script (in editor)</param>
        /// <param name="udon">runtime script</param>
        private static void SavePlaylistToUdon(KinelPlaylistScript behaviour, KinelPlaylist udon)
        {
            var urls = new List<VRCUrl>();
            var titles = new List<string>();
            var types = new List<KinelMediaType>();
            var playlistNames = new List<string>();
            var playlistIndex = new List<int>();

            foreach (var playlistItem in behaviour.playlists)
            {
                if (playlistItem == null) continue;
                playlistIndex.Add(urls.Count);
                playlistNames.Add(playlistItem.playlistName);
                foreach (var track in playlistItem.Tracks)
                {
                    if (track == null) continue;
                    urls.Add(track.Url);
                    titles.Add(track.Title);
                    types.Add(track.Type);
                }
            }

            udon.SetProgramVariable("playlistNames", playlistNames.ToArray());
            udon.SetProgramVariable("urls", urls.ToArray());
            udon.SetProgramVariable("titles", titles.ToArray());
            udon.SetProgramVariable("types", types.ToArray());
            udon.SetProgramVariable("playlistIndex", playlistIndex.ToArray());
            UdonSharpEditorUtility.CopyProxyToUdon(udon);
            AssetDatabase.SaveAssets();
        }

        #endregion
    }
}