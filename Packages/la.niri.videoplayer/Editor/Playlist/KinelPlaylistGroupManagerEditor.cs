using System;
using System.Reflection;
using Kinel.VideoPlayer.Scripts;
using Kinel.VideoPlayer.Udon.Controller;
using Kinel.VideoPlayer.Udon.Playlist;
using UdonSharp;
using UdonSharpEditor;
using UnityEditor;
using UnityEditor.Events;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using VRC.Udon;
using Object = System.Object;

namespace Kinel.VideoPlayer.Editor
{
    [CustomEditor(typeof(KinelPlaylistGroupManagerScript))]
    public class KinelPlaylistGroupManagerEditor : KinelEditorBase
    {

        private KinelPlaylistGroupManagerScript _kinelPlaylistGroupManager;
        private SerializedProperty _videoPlayer, _playlists, _storePlaylist;
        
        
        private ReorderableList _reorderableList;
        
        
        public void OnEnable()
        {
            _kinelPlaylistGroupManager = target as KinelPlaylistGroupManagerScript;
            _videoPlayer = serializedObject.FindProperty(nameof(KinelPlaylistGroupManagerScript.kinelVideoPlayer));
            _playlists = serializedObject.FindProperty(nameof(KinelPlaylistGroupManagerScript.playlists));
            _storePlaylist = serializedObject.FindProperty(nameof(KinelPlaylistGroupManagerScript.storePlaylist));
            // _playlistTabController = serializedObject.FindProperty(nameof(KinelPlaylistGroupManagerScript.controller));

            _reorderableList = new ReorderableList(serializedObject, _playlists)
            {
                drawElementCallback = (rect, index, active, focused) =>
                {
                    var uiRect = new Rect(rect.x, rect.y + 5, rect.width, EditorGUIUtility.singleLineHeight);
                    EditorGUI.PropertyField(uiRect , _playlists.GetArrayElementAtIndex(index), new GUIContent(){text = $"Playlist name {index}"});
                    uiRect.y += uiRect.height + 5;
                },
                elementHeightCallback = index => (EditorGUIUtility.singleLineHeight + 10),
                
                drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Playlist list"),

            };
            
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            serializedObject.Update();

            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                _reorderableList.DoLayoutList();
                

                if (GUILayout.Button("Generate"))
                {
                    GeneratePlaylistGroup();
                }

                if (_storePlaylist.boolValue)
                {
                    if (GUILayout.Button("Delete"))
                    {
                        
                        var isDelete = EditorUtility.DisplayDialog("Playlist (tab)", "本当に削除しますか?", "はい", "いいえ");
                        if (!isDelete)
                        {
                            EditorUtility.DisplayDialog("Playlist (tab)", "キャンセルされました、", "ok");
                            return;
                        }
                        DeletePlaylistGroup();
                    }
                }
                
            }
            EditorGUILayout.EndVertical();


            if (serializedObject.ApplyModifiedProperties())
            {
                GetPlaylistTabController();
            }


        }

        public override void ApplyUdonProperties()
        {
        }

        public void GeneratePlaylistGroup()
        {
            if (_playlists.arraySize == 0)
                return;
            
            GetPlaylistTabController();
            
            var playlistImpl =
                _kinelPlaylistGroupManager.gameObject.transform.Find("Playlist/INTERNAL/KineLVP Playlist").gameObject;
            var playlistItem =
                _kinelPlaylistGroupManager.gameObject.transform.Find("KinelVP TabList Impl/Tablist/Scroll View/Viewport/TabItemPrefab").gameObject;
            var content =
                _kinelPlaylistGroupManager.gameObject.transform.Find("KinelVP TabList Impl/Tablist/Scroll View/Viewport/Content");
            var playlistParent =
                _kinelPlaylistGroupManager.gameObject.transform.Find("Playlist");

            if (content.childCount != 0)
            {
                // var success = EditorUtility.DisplayDialog("Playlist", "既にリストがあります。削除して生成しますか?", "はい", "いいえ");
                var select = EditorUtility.DisplayDialogComplex("Playlist (tab)", "既にリストがあります。追加して生成しますか?", "はい", "キャンセル", "全て削除して追加");
                switch (select)
                {
                    case 0:
                        AddPlaylist();
                        return;
                    case 1:
                        Debug.Log($"{DEBUG_LOG_PREFIX} playlist generate canceled.");
                        return;
                    case 2:
                        DeletePlaylistGroup();
                        Debug.Log($"{DEBUG_LOG_PREFIX} playlist deleted.");
                        break;
                    default:
                        Debug.Log($"{DEBUG_LOG_PREFIX} playlist generate canceled.");
                        return;
                }
            }

            for (int i = 0; i < _playlists.arraySize; i++)
            {
                var playlistName = _playlists.GetArrayElementAtIndex(i).stringValue;
                GameObject tabPrefab = Instantiate(playlistItem);
                GameObject playlistPrefab = Instantiate(playlistImpl);
                /// TAB
                tabPrefab.transform.SetParent(content);
                tabPrefab.transform.localPosition = Vector3.zero;
                tabPrefab.transform.localRotation = Quaternion.identity;
                tabPrefab.transform.localScale    = Vector3.one;
                var text = tabPrefab.transform.Find("Text").GetComponent<Text>();
                text.text = playlistName;
                tabPrefab.SetActive(true);
                tabPrefab.name = $"{i}_{playlistName}";
                
                /// PLAYLIST
                playlistPrefab.transform.SetParent(playlistParent);
                playlistPrefab.transform.localPosition = Vector3.zero;
                playlistPrefab.transform.localRotation = Quaternion.identity;
                playlistPrefab.transform.localScale    = Vector3.one;
                playlistPrefab.SetActive(true);
                playlistPrefab.name = $"{i}_{playlistName}";
                
                PlaylistButtonInitializer(tabPrefab, playlistPrefab, content.gameObject);
                
                
                
                //// isOn処理 (後で変更できるようにする(予定))
                tabPrefab.transform.Find("Dummy").GetComponent<Toggle>().isOn = (i == 0);
                // if (i != 0)
                // {
                //     playlistPrefab.transform.Find("KVPPlaylist/Canvas").gameObject.SetActive(false);
                // }

                ApplyPlaylistProperties(playlistPrefab, (i == 0));

            }
            
            playlistItem.SetActive(false);
            playlistImpl.SetActive(false);

            _storePlaylist.boolValue = true;
            
            
            Debug.Log($"{DEBUG_LOG_PREFIX} Generate playlistgroup.");

        }

        // 追加分生成
        private void AddPlaylist()
        {
            var playlistImpl =
                _kinelPlaylistGroupManager.gameObject.transform.Find("Playlist/INTERNAL/KineLVP Playlist").gameObject;
            var playlistItem =
                _kinelPlaylistGroupManager.gameObject.transform.Find("KinelVP TabList Impl/Tablist/Scroll View/Viewport/TabItemPrefab").gameObject;
            var content =
                _kinelPlaylistGroupManager.gameObject.transform.Find("KinelVP TabList Impl/Tablist/Scroll View/Viewport/Content");
            var playlistParent =
                _kinelPlaylistGroupManager.gameObject.transform.Find("Playlist");

            var count = content.childCount;
            var zoubun = _playlists.arraySize - count;

            if (zoubun > 0)
            {
                for (int i = count; i < _playlists.arraySize; i++)
                { 
//////// 書き直し
                    var playlistName = _playlists.GetArrayElementAtIndex(i).stringValue;
                    GameObject tabPrefab = Instantiate(playlistItem);
                    GameObject playlistPrefab = Instantiate(playlistImpl);
                    /// TAB
                    tabPrefab.transform.SetParent(content);
                    tabPrefab.transform.localPosition = Vector3.zero;
                    tabPrefab.transform.localRotation = Quaternion.identity;
                    tabPrefab.transform.localScale    = Vector3.one;
                    var text = tabPrefab.transform.Find("Text").GetComponent<Text>();
                    text.text = playlistName;
                    tabPrefab.SetActive(true);
                    tabPrefab.name = $"{i}_{playlistName}";
                
                    /// PLAYLIST
                    playlistPrefab.transform.SetParent(playlistParent);
                    playlistPrefab.transform.localPosition = Vector3.zero;
                    playlistPrefab.transform.localRotation = Quaternion.identity;
                    playlistPrefab.transform.localScale    = Vector3.one;
                    playlistPrefab.SetActive(true);
                    playlistPrefab.name = $"{i}_{playlistName}";
                
                    PlaylistButtonInitializer(tabPrefab, playlistPrefab, content.gameObject);
                
                
                
                    //// isOn処理 (後で変更できるようにする(予定))
                    tabPrefab.transform.Find("Dummy").GetComponent<Toggle>().isOn = (i == 0);
                    // playlistPrefab.transform.Find("KVPPlaylist/Canvas").gameObject.SetActive(false);

                    ApplyPlaylistProperties(playlistPrefab, (i == 0));

                }
            }
            else if(zoubun < 0)
            {
                var isDelete = EditorUtility.DisplayDialog("Playlist (tab)", "前回生成分と比較してプレイリストが減少しています。削除したプレイリストをシーン上から削除しますか?", "はい", "いいえ");

                if (!isDelete)
                {
                    Debug.Log($"{DEBUG_LOG_PREFIX} playlist generate canceled.");
                    return;
                }

                for (int i = _playlists.arraySize - 1; content.childCount == _playlists.arraySize; i--)
                {
                    DestroyImmediate(content.GetChild(i).gameObject);
                    DestroyImmediate(playlistParent.GetChild(i).gameObject);
                }

            }

        }


        public void ApplyPlaylistProperties(GameObject playlist, bool active)
        {
            var hoge = playlist.GetComponentInChildren<KinelPlaylistScript>();
            Debug.Log($"hoge is null {hoge == null}");
            var editor = UnityEditor.Editor.CreateEditor(playlist.GetComponent<KinelPlaylistScript>(), typeof(KinelPlaylistEditor)) as KinelPlaylistEditor;
            editor.ApplyUdonProperties();
                
            playlist.transform.Find("KVPPlaylist/Canvas").gameObject.SetActive(active);
            
            
        }

        private void PlaylistButtonInitializer(GameObject prefab, GameObject playlistPrefab, GameObject content)
        {
            // if (_playlistTabController.objectReferenceValue == null)
            // {
            //     GetPlaylistTabController();
            //     if (_playlistTabController.objectReferenceValue == null)
            //     {
            //         EditorUtility.DisplayDialog("Error", "Editor error. Please contact me via Twitter @ni_rilana.", "OK");
            //         Debug.LogError($"{DEBUG_ERROR_PREFIX} Editor error. Please contact me via Twitter @ni_rilana. ");
            //         return;
            //     }
            // }
            
            var toggle = prefab.transform.Find("Dummy").GetComponent<Toggle>();
            var toggleGroup = content.GetComponent<ToggleGroup>();
            toggle.group = toggleGroup;
            toggleGroup.RegisterToggle(toggle);
            var playlistCanvas = playlistPrefab.transform.Find("KVPPlaylist/Canvas").gameObject;

            MethodInfo canvasSetterMethodInfo = typeof(GameObject).GetMethod("SetActive");
            UnityAction<bool> methodDelegate= System.Delegate.CreateDelegate(typeof(UnityAction<bool>), playlistCanvas, canvasSetterMethodInfo) as UnityAction<bool>;
            UnityEventTools.AddPersistentListener(toggle.onValueChanged, methodDelegate);
            EditorUtility.SetDirty(toggle.gameObject);
        }

        public void DeletePlaylistGroup()
        {
            var content =
                _kinelPlaylistGroupManager.gameObject.transform.Find("KinelVP TabList Impl/Tablist/Scroll View/Viewport/Content");
            var playlistParent =
                _kinelPlaylistGroupManager.gameObject.transform.Find("Playlist");
            var playlistItem =
                _kinelPlaylistGroupManager.gameObject.transform.Find("KinelVP TabList Impl/Tablist/Scroll View/Viewport/TabItemPrefab").gameObject;
            var playlistImpl =
                _kinelPlaylistGroupManager.gameObject.transform.Find("Playlist/INTERNAL/KineLVP Playlist").gameObject;

            var count = content.childCount;
            for (int i = 0; i < count; i++)
            {
                DestroyImmediate(content.GetChild(0).gameObject);
                DestroyImmediate(playlistParent.GetChild(1).gameObject);
            }
            
            playlistItem.SetActive(true);
            playlistImpl.SetActive(true);
            
            Debug.Log($"{DEBUG_LOG_PREFIX} Playlistgroup has deleted.");
            
        }

        private void GetPlaylistTabController()
        {
            // if (_playlistTabController.objectReferenceValue != null)
            //     return;

            // var tabController = _kinelPlaylistGroupManager.GetUdonSharpComponentInChildren<KinelPlaylistTabController>();
            //
            // _playlistTabController.objectReferenceValue = tabController;
        }

        

    }
}