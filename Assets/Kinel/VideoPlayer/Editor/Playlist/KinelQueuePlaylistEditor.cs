using System;
using System.Collections.Generic;
using Kinel.VideoPlayer.Editor.Internal;
using Kinel.VideoPlayer.Scripts;
using Kinel.VideoPlayer.Scripts.Parameter;
using Kinel.VideoPlayer.Udon;
using Kinel.VideoPlayer.Udon.Playlist;
using UdonSharpEditor;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase;

namespace Kinel.VideoPlayer.Editor
{
    [CustomEditor(typeof(KinelQueuePlaylistScript))]
    public class KinelQueuePlaylistEditor : KinelEditorBase
    {

        private KinelQueuePlaylistScript _playlist;
        private SerializedProperty _kinelVideoPlayer, _isAutoFill;
        
        public void OnEnable()
        {
            _playlist = target as KinelQueuePlaylistScript;
            _kinelVideoPlayer = serializedObject.FindProperty(nameof(KinelQueuePlaylistScript.videoPlayer));
            _isAutoFill = serializedObject.FindProperty(nameof(KinelQueuePlaylistScript.isAutoFill));

        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            serializedObject.Update();
            
            
            // playlist header
            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                EditorGUILayout.LabelField("KineL Queue Playlist");
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
                KinelEditorUtilities.DrawFillMessage(_isAutoFill.enumValueIndex);
                EditorGUILayout.Space();
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();
            
            base.DrawFooter();
            
            if (serializedObject.ApplyModifiedProperties())
            {
                ApplyPlaylistProperties();
                ApplyUdonProperties();
            }
        }

        public override void ApplyUdonProperties()
        {
            var playlist = _playlist.GetUdonSharpComponentInChildren<KinelQueuePlaylist>();

            if (playlist == null)
            {
                Debug.LogError($"{DEBUG_ERROR_PREFIX} playlist null");
                return;
            }
            Undo.RecordObject(playlist, "Instance attached");
            playlist.UpdateProxy();
            playlist.SetProgramVariable("videoPlayer", _kinelVideoPlayer.objectReferenceValue);
            playlist.ApplyProxyModifications();
            UdonSharpEditorUtility.CopyProxyToUdon(playlist);
            KinelEditorUtilities.UpdateKinelVideoUIComponents(playlist);
            EditorUtility.SetDirty(playlist);
            // AssetDatabase.SaveAssets();
            
            

        }

        private void ApplyPlaylistProperties()
        {
            
        }

        private void AutoFillProperties()
        {
            
            _isAutoFill.enumValueIndex = (int) KinelEditorUtilities.FillUdonSharpInstance<KinelVideoPlayer>(ref _kinelVideoPlayer, _playlist.gameObject, false);
        }
        
        

        
    }
}