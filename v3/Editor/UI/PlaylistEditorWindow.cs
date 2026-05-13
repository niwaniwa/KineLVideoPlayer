using System.Collections.Generic;
using Editor;
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
        private bool _isDirty;

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

            // Track 追加・削除でメタラベルを更新し dirty にする (一度だけ購読)
            _trackList.itemsAdded += OnTrackListChanged;
            _trackList.itemsRemoved += OnTrackListChanged;

            // Any bound serialized property change marks dirty
            rootVisualElement.RegisterCallback<SerializedPropertyChangeEvent>(_ => MarkDirty());

            // Playlist name field updates sidebar / meta on edit
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

            // Header: name + meta
            var nameProp = _selectedItemSO.FindProperty(nameof(KinelPlaylistItem.playlistName));
            _playlistNameField.BindProperty(nameProp);
            UpdateMetaLabel(playlistItem);

            // Track list
            var tracksProp = _selectedItemSO.FindProperty(nameof(KinelPlaylistItem.Tracks));
            _trackList.makeItem = MakeTrackRow;
            _trackList.bindItem = (element, index) => BindTrackRow(element, index, tracksProp);
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
            _playlistMetaLabel.text = $"{item.Tracks.Count} tracks ﾂｷ {string.Join(", ", parts)}";
        }

        // ============================================================
        //  Track row (custom layout: Type chip + Title + URL)
        // ============================================================
        private VisualElement MakeTrackRow()
        {
            var row = new VisualElement();
            row.AddToClassList("kinel-track-row");

            var header = new VisualElement();
            header.AddToClassList("kinel-track-header");

            var typeField = new EnumField(KinelMediaType.AvPro);
            typeField.AddToClassList("kinel-type-chip");
            typeField.name = "track-type";
            header.Add(typeField);

            row.Add(header);

            var fields = new VisualElement();
            fields.AddToClassList("kinel-track-fields");

            var titleField = new TextField("Title") { name = "track-title" };
            fields.Add(titleField);

            var urlField = new TextField("URL") { name = "track-url" };
            fields.Add(urlField);

            row.Add(fields);

            // Type change: update color chip class + dirty
            // 注意: binding 解放時 (アイテム削除など) にも newValue=null で発火する。
            // 値型 enum への直接キャストを避け、is パターンで null 安全にする。
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

        private void BindTrackRow(VisualElement element, int index, SerializedProperty tracksProp)
        {
            if (index < 0 || index >= tracksProp.arraySize) return;
            var elemProp = tracksProp.GetArrayElementAtIndex(index);

            var typeField = element.Q<EnumField>("track-type");
            var titleField = element.Q<TextField>("track-title");
            var urlField = element.Q<TextField>("track-url");

            var typeProp = elemProp.FindPropertyRelative("Type");
            var titleProp = elemProp.FindPropertyRelative("Title");
            var urlProp = elemProp.FindPropertyRelative("Url");
            // VRCUrl の内部 string フィールド (VRCSDK のバージョンで "m_Url" or "url")
            var urlStringProp = urlProp != null
                ? (urlProp.FindPropertyRelative("m_Url") ?? urlProp.FindPropertyRelative("url"))
                : null;

            if (typeField != null && typeProp != null)
            {
                var current = (KinelMediaType)typeProp.intValue;
                typeField.BindProperty(typeProp);
                // BindProperty は値変化がないと表示を更新しないことがあるため明示的に同期反映する。
                // SerializedProperty を遅延クロージャで掴むと配列削除時に NRE になるので value のみ捕捉。
                typeField.SetValueWithoutNotify(current);
                ApplyTypeChipClass(typeField, current);
            }

            if (titleField != null && titleProp != null) titleField.BindProperty(titleProp);
            if (urlField != null && urlStringProp != null) urlField.BindProperty(urlStringProp);
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

        private void OnTrackListChanged(IEnumerable<int> _)
        {
            // ListView 側の配列操作直後に呼ばれる。SO の状態と UI を再同期する。
            _selectedItemSO?.ApplyModifiedProperties();
            _selectedItemSO?.Update();
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