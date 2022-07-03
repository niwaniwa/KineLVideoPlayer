using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kinel.VideoPlayer.Editor.Internal;
using Kinel.VideoPlayer.Scripts;
using Kinel.VideoPlayer.Scripts.Parameter;
using Kinel.VideoPlayer.Udon;
using Kinel.VideoPlayer.Udon.Playlist;
using UdonSharp;
using UdonSharpEditor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using VRC.SDKBase;
using VRC.Udon;

namespace Kinel.VideoPlayer.Editor
{
    [CustomEditor(typeof(KinelPlaylistScript))]
    public class KinelPlaylistEditor : KinelEditorBase
    {
      
        private KinelPlaylistScript _playlist;

        private SerializedProperty _kinelVideoPlayer;
        private SerializedProperty _videos, _autoplay, _loop, _playlistUrl, _generateInUnity, _showPlaylist, _storePlaylist, _isAutoFill, _managePlaylistTabHost;
        private SerializedProperty _autoPlayWhenJoin, _nextPlayVideoWhenPlaySelected, _shuffle;


        private ReorderableList _reorderableList;

        private void OnEnable()
        {
            _playlist = target as KinelPlaylistScript;

            _kinelVideoPlayer = serializedObject.FindProperty(nameof(KinelPlaylistScript.videoPlayer));
            _videos = serializedObject.FindProperty(nameof(KinelPlaylistScript.videoDatas));
            _autoPlayWhenJoin = serializedObject.FindProperty(nameof(KinelPlaylistScript.autoPlayWhenJoin));
            _nextPlayVideoWhenPlaySelected = serializedObject.FindProperty(nameof(KinelPlaylistScript.nextPlayVideoWhenPlaySelected));
            _shuffle = serializedObject.FindProperty(nameof(KinelPlaylistScript.shuffle));
            _autoplay = serializedObject.FindProperty(nameof(KinelPlaylistScript.autoPlay));
            _loop = serializedObject.FindProperty(nameof(KinelPlaylistScript.loop));
            _playlistUrl = serializedObject.FindProperty(nameof(KinelPlaylistScript.playlistUrl));
            _generateInUnity = serializedObject.FindProperty(nameof(KinelPlaylistScript.generateInUnity));
            _showPlaylist = serializedObject.FindProperty(nameof(KinelPlaylistScript.showPlaylist));
            _storePlaylist = serializedObject.FindProperty(nameof(KinelPlaylistScript.storePlaylist));
            _isAutoFill = serializedObject.FindProperty(nameof(KinelPlaylistScript.isAutoFill));
            _managePlaylistTabHost = serializedObject.FindProperty(nameof(KinelPlaylistScript.managePlaylistTabHost));

            _reorderableList = new ReorderableList(serializedObject, _videos)
            {
                drawElementCallback = (rect, index, active, focused) =>
                {
                    EditorGUI.PropertyField(rect, _videos.GetArrayElementAtIndex(index));
                },
                elementHeightCallback = index => EditorGUI.GetPropertyHeight(_videos.GetArrayElementAtIndex(index)),
                
                drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Video Playlist (PlayMode 0=Video, 1=Stream(Live) )"),

            };
        }

       

        public override void OnInspectorGUI()
        {
            
            base.OnInspectorGUI();

            serializedObject.Update();

            // if (_managePlaylistTabHost.boolValue)
            // {
            //     EditorGUILayout.BeginVertical(GUI.skin.box);
            //     {
            //         EditorGUILayout.LabelField("Playlist is controlled by TabManager.");
            //     }
            //     EditorGUILayout.EndVertical();
            //     return;
            // }
            
            // playlist header
            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                EditorGUILayout.LabelField("KineL Playlist");
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();
            
            // playlist main
            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                EditorGUILayout.LabelField("基本設定");
                EditorGUI.indentLevel++;
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(_kinelVideoPlayer);
                AutoFillProperties();
                if (_isAutoFill.enumValueIndex == (int) FillResult.Success)
                {
                    EditorGUILayout.HelpBox("自動的に設定されました。", MessageType.Info);
                } 
                else if (_isAutoFill.enumValueIndex == (int) FillResult.AlreadyExistence)
                {
                    EditorGUILayout.HelpBox("設定されました。", MessageType.Info);
                }
                
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            
            PlaylistSettingsInspector();
            
            EditorGUILayout.Space();
            
            PlaylistToolInspector();
            
            EditorGUILayout.Space();
            
            PlaylistInspectror();
            
            EditorGUILayout.Space();
            
            base.DrawFooter();

            if (serializedObject.ApplyModifiedProperties())
            {
                ApplyPlaylistProperties();
                ApplyUdonProperties();
            }
                
            
        }

        public void PlaylistToolInspector()
        {

            // playlist tool
            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                EditorGUILayout.LabelField("Playlist tool");
                EditorGUI.indentLevel++;
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(_playlistUrl);
                EditorGUILayout.Space();
                if (GUILayout.Button("Import"))
                {
                    var data = PlaylistDataGateway.GetYoutubePlaylist(_playlistUrl.stringValue);
                    Debug.Log($"{DEBUG_LOG_PREFIX} Import {_playlistUrl.stringValue}, {data.name}");

                    _videos.arraySize = 0;
                    for (int i = 0; i < data.videos.Length; i++)
                    {
                        _videos.arraySize++;
                        _videos.GetArrayElementAtIndex(i).FindPropertyRelative("title").stringValue = data.videos[i].title;
                        _videos.GetArrayElementAtIndex(i).FindPropertyRelative("url").stringValue = data.videos[i].url;
                        _videos.GetArrayElementAtIndex(i).FindPropertyRelative("mode").intValue = data.videos[i].mode;
                       
                    }
                    
                    _showPlaylist.boolValue = false;
                    EditorGUILayout.Space();
                    
                    Debug.Log($"{DEBUG_LOG_PREFIX} Complete");

                }
                
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();
        }

        public void PlaylistSettingsInspector()
        {
            // playlist settings
            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                EditorGUILayout.LabelField("Settings");
                EditorGUI.indentLevel++;
                EditorGUILayout.Space();
                // autoplay loop
                EditorGUILayout.PropertyField(_autoPlayWhenJoin, new GUIContent("Join時に再生"));
                EditorGUILayout.PropertyField(_nextPlayVideoWhenPlaySelected, new GUIContent("次の動画を自動再生"));
                EditorGUILayout.PropertyField(_shuffle, new GUIContent("シャッフル再生"));
                EditorGUILayout.PropertyField(_loop);
                EditorGUILayout.Space();
                
                // EditorGUILayout.BeginVertical(GUI.skin.box);
                // {
                //     EditorGUILayout.HelpBox("Tips: Unity内でリストアイテムを生成します。各動画の項目を編集したい場合に使用してください。チェックをつけない場合はVRC内で自動的に生成されます。", MessageType.Info);
                //     EditorGUILayout.PropertyField(_generateInUnity);
                // }
                // EditorGUILayout.EndVertical();
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
            }
            EditorGUILayout.EndVertical();
        }

        public void PlaylistInspectror()
        {
            // playlist list
            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                EditorGUILayout.LabelField("Playlist");
                EditorGUI.indentLevel++;
                
                if (true/*_generateInUnity.boolValue*/)
                {
                    EditorGUILayout.HelpBox("Tips: アップロードする際は必ず押してください。", MessageType.Info);
                    EditorGUILayout.Space();
                    if (GUILayout.Button("Generate playlist"))
                    {
                        CreatePlaylistItem();
                        _storePlaylist.boolValue = true;
                    }

                    if (_storePlaylist.boolValue)
                    {
                        if (GUILayout.Button("Delete playlist items"))
                        {
                            ClearPlaylistItem();
                            _storePlaylist.boolValue = false;
                        }
                    }
                }
                
                EditorGUILayout.Space();

                _showPlaylist.boolValue = EditorGUILayout.Foldout(_showPlaylist.boolValue, "Video list");
                if (_showPlaylist.boolValue)
                {
                    _reorderableList.DoLayoutList();
                }
                
                if (GUILayout.Button("Playlist data delete"))
                {
                    ClearPlaylistData();
                }

                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();
        }

        public void CreatePlaylistItem()
        {

            var playlistItem =
                _playlist.gameObject.transform.Find("KVPPlaylist/Canvas/PlaylistImpl/Scroll View/Viewport/Prefabs/PlaylistItem").gameObject;
            var content =
                _playlist.gameObject.transform.Find("KVPPlaylist/Canvas/PlaylistImpl/Scroll View/Viewport/Content");
            for (var i = 0; i < _playlist.videoDatas.Length; i++)
            {
                GameObject prefab = Instantiate(playlistItem);

                prefab.name = $"Video ({i})";
                prefab.transform.SetParent(content);
                prefab.transform.localPosition = Vector3.zero;
                prefab.transform.localRotation = Quaternion.identity;
                prefab.transform.localScale    = Vector3.one;
                prefab.SetActive(true);

                var descriptionComponent = prefab.transform.Find("Description").GetComponent<Text>();
                descriptionComponent.text = _playlist.videoDatas[i].title;
            }
            playlistItem.SetActive(false);
        }

        public void ClearPlaylistItem()
        {
            var content = _playlist.gameObject.transform.Find("KVPPlaylist/Canvas/PlaylistImpl/Scroll View/Viewport/Content");
            var count = content.childCount;
            for (int i = 0; i < count; i++)
            {
                DestroyImmediate(content.GetChild(0).gameObject);
            }

        }

        public void ClearPlaylistData()
        {
            Undo.RecordObject(_playlist, "Delete playlist data");
            _videos.arraySize = 0;
            EditorUtility.SetDirty(_playlist);
        }
        
        public override void ApplyUdonProperties()
        {
            var playlist = _playlist.GetUdonSharpComponentInChildren<KinelPlaylist>();
            // var playlistUdon = _playlist.GetUdonSharpComponentInChildren<KinelPlaylist>();
            // var playlistSerialize = new SerializedObject(playlist);
            
            if (playlist == null)
            {
                Debug.LogError($"{DEBUG_ERROR_PREFIX} playlist null");
                return;
            }
            
            // Debug.Log($"{DEBUG_LOG_PREFIX} Attached playlist data.");
            
            Undo.RecordObject(playlist, "Properties attached");
            var titles = new List<string>();
            var urls = new List<VRCUrl>();
            var playMode = new List<int>();
            
            for (int i = 0; i < _videos.arraySize; i++)
            {
                titles.Add(_videos.GetArrayElementAtIndex(i).FindPropertyRelative("title").stringValue);
                urls.Add(new VRCUrl(_videos.GetArrayElementAtIndex(i).FindPropertyRelative("url").stringValue));
                playMode.Add(_videos.GetArrayElementAtIndex(i).FindPropertyRelative("mode").intValue);
            }
            playlist.UpdateProxy();
            playlist.SetProgramVariable("videoPlayer", _kinelVideoPlayer.objectReferenceValue);
            playlist.SetProgramVariable("titles", titles.ToArray());
            playlist.SetProgramVariable("urls", urls.ToArray());
            playlist.SetProgramVariable("playMode", playMode.ToArray());
            playlist.SetProgramVariable("autoPlayMode", _autoplay.enumValueIndex);
            playlist.SetProgramVariable("loopMode", _loop.enumValueIndex);
            playlist.SetProgramVariable("autoPlayWhenJoin", _autoPlayWhenJoin.boolValue);
            playlist.SetProgramVariable("nextPlayVideoWhenPlaySelected", _nextPlayVideoWhenPlaySelected.boolValue);
            playlist.SetProgramVariable("shuffle", _shuffle.boolValue);
            playlist.ApplyProxyModifications();
            UdonSharpEditorUtility.CopyProxyToUdon(playlist);
            
            KinelEditorUtilities.UpdateKinelVideoUIComponents(playlist, _kinelVideoPlayer.objectReferenceValue);
            EditorUtility.SetDirty(playlist);
        }

        private void ApplyPlaylistProperties()
        {
            
        }

        private void AutoFillProperties()
        {
            _isAutoFill.enumValueIndex = (int) KinelEditorUtilities.FillUdonSharpInstance<KinelVideoPlayer>(_kinelVideoPlayer, _playlist.gameObject, false);
        }
        
            
    }
}