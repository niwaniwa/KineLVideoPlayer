
using Kinel.VideoPlayer.Editor.Internal;
using Kinel.VideoPlayer.Scripts;
using Kinel.VideoPlayer.Scripts.Parameter;
using Kinel.VideoPlayer.Udon;
using Kinel.VideoPlayer.Udon.Playlist;
using UdonSharpEditor;
using UnityEditor;
using UnityEngine;

namespace Kinel.VideoPlayer.Editor.Playlist
{
    
    [CustomEditor(typeof(KinelVideoHistoryScript))]
    public class KinelVideoHistoryEditor : KinelEditorBase
    {
        
        private KinelVideoHistoryScript _playlist;

        private SerializedProperty _kinelVideoPlayer;
        private SerializedProperty _isAutoFill, _saveErrorUrl;

        public void OnEnable()
        {
             _playlist = target as KinelVideoHistoryScript;;

            _kinelVideoPlayer = serializedObject.FindProperty(nameof(KinelVideoHistoryScript.videoPlayer));
            _isAutoFill = serializedObject.FindProperty(nameof(KinelVideoHistoryScript.isAutoFill));
            _saveErrorUrl = serializedObject.FindProperty(nameof(KinelVideoHistoryScript.saveErrorUrl));
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();
            
            // playlist header
            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                EditorGUILayout.LabelField("KineL Video History");
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();
            
            // playlist main
            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                EditorGUILayout.LabelField("基本設定");
                EditorGUI.indentLevel++;
                EditorGUILayout.Space();
                _isAutoFill.enumValueIndex = (int) KinelEditorUtilities.FillUdonSharpInstance<KinelVideoPlayer>(ref _kinelVideoPlayer, _playlist.gameObject, false);
                if (_isAutoFill.enumValueIndex == 0)
                    EditorGUILayout.LabelField(_kinelVideoPlayer.displayName, "自動設定されました。");
                else
                    EditorGUILayout.PropertyField(_kinelVideoPlayer);
                EditorGUILayout.Space();
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();
            
            if (serializedObject.ApplyModifiedProperties())
            {
                ApplyUdonProperties();
            }
        }
        
        public override void ApplyUdonProperties()
        {
            var playlist = _playlist.GetUdonSharpComponentInChildren<KinelVideoHistory>();

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

    }
}