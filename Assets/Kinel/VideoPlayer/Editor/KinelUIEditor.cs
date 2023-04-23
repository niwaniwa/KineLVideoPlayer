using System;
using Kinel.VideoPlayer.Editor.Internal;
using Kinel.VideoPlayer.Scripts;
using Kinel.VideoPlayer.Udon;
using UdonSharpEditor;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase.Editor.BuildPipeline;

namespace Kinel.VideoPlayer.Editor
{
    [CustomEditor(typeof(KinelUIScript))]
    public class KinelUIEditor : KinelEditorBase
    {

        private KinelUIScript _kinelVideoPlayerUI;
        private SerializedProperty _kinelVideoPlayer, _isAutoFill;
        
        public void OnEnable()
        {
            _kinelVideoPlayerUI = target as KinelUIScript;
            _kinelVideoPlayer = serializedObject.FindProperty(nameof(KinelUIScript.videoPlayer));
            _isAutoFill = serializedObject.FindProperty(nameof(KinelUIScript.isAutoFill));
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();
            
            // UI header
            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                EditorGUILayout.LabelField("KineL UI Base");
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("基本設定");
                EditorGUI.indentLevel++;
                EditorGUILayout.Space();
                _isAutoFill.enumValueIndex = (int) KinelEditorUtilities.FillUdonSharpInstance<KinelVideoPlayer>(ref _kinelVideoPlayer, _kinelVideoPlayerUI.gameObject, false);
                if (_kinelVideoPlayer.objectReferenceValue)
                    EditorGUILayout.LabelField(_kinelVideoPlayer.displayName, "自動設定されました。");
                else
                    EditorGUILayout.PropertyField(_kinelVideoPlayer);
                
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
            var ui = _kinelVideoPlayerUI.GetComponentInChildren<KinelVideoPlayerUI>();
            if (ui == null)
            {
                Debug.LogError($"{DEBUG_ERROR_PREFIX} UI null");
                return;
            }
            Undo.RecordObject(_kinelVideoPlayerUI, "Instance attached");
            ui.UpdateProxy();
            ui.SetProgramVariable("videoPlayer", _kinelVideoPlayer.objectReferenceValue);
            ui.ApplyProxyModifications();
            UdonSharpEditorUtility.CopyProxyToUdon(ui);
            
            EditorUtility.SetDirty(_kinelVideoPlayerUI);
            // _kinelVideoPlayerUI.UpdateProxy();
        }
        
        
    }
}