using System;
using Kinel.VideoPlayer.Scripts;
using Kinel.VideoPlayer.Udon;
using UdonSharpEditor;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase.Editor.BuildPipeline;

namespace Kinel.VideoPlayer.Editor
{
    [CustomEditor(typeof(KinelVideoPlayerUI))]
    public class KinelUIEditor : KinelEditorBase
    {

        private KinelVideoPlayerUI _kinelVideoPlayerUI;
        private SerializedProperty _kinelVideoPlayer, _isAutoFill;
        
        public void OnEnable()
        {
            _kinelVideoPlayerUI = target as KinelVideoPlayerUI;
            _kinelVideoPlayer = serializedObject.FindProperty(nameof(KinelVideoPlayerUI.videoPlayer));
            _isAutoFill = serializedObject.FindProperty(nameof(KinelVideoPlayerUI.isAutoFill));
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            if (UdonSharpGUI.DrawConvertToUdonBehaviourButton(target) ||
                UdonSharpGUI.DrawProgramSource(target))
                return;
            
            serializedObject.Update();
            
            
            // UI header
            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                EditorGUILayout.LabelField("KineL UI Base");
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("基本設定");
                EditorGUI.indentLevel++;
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(_kinelVideoPlayer);
                AutoFillProperties();
                if (_isAutoFill.boolValue)
                {
                    EditorGUILayout.HelpBox("自動的に設定されました。", MessageType.Info);
                }
                
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();


            if (serializedObject.ApplyModifiedProperties())
            {
                ApplyUdonProperties();
            }
        }
        
        public void AutoFillProperties()
        {
            if (_kinelVideoPlayer.objectReferenceValue == null)
            {

                var playerScripts = GetVideoPlayers(); //
                if (playerScripts.Length != 0)
                {
                    if (playerScripts.Length != 1)
                        return;

                    var system = playerScripts[0].gameObject.GetUdonSharpComponentsInChildren<KinelVideoPlayer>();
                    if (system.Length >= 2 || system.Length == 0)
                        return;

                    _isAutoFill.boolValue = true;
                    Undo.RecordObject(_kinelVideoPlayerUI, "Instance attached");
                    _kinelVideoPlayer.objectReferenceValue = system[0]; // 時々nullになる
                    _kinelVideoPlayer.serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(_kinelVideoPlayerUI);
                    // _kinelVideoPlayerUI.SetProgramVariable("videoPlayer", _kinelVideoPlayer.objectReferenceValue);
                    // _kinelVideoPlayerUI.videoPlayer = system[0];

                }
                
                // Debug.LogError($"null? { _kinelVideoPlayer.objectReferenceValue == null}");

            }

        }

        public override void ApplyUdonProperties()
        {
            Undo.RecordObject(_kinelVideoPlayerUI, "Instance attached");
            _kinelVideoPlayerUI.UpdateProxy();
            _kinelVideoPlayerUI.SetProgramVariable("videoPlayer", _kinelVideoPlayer.objectReferenceValue);
            _kinelVideoPlayerUI.ApplyProxyModifications();
            UdonSharpEditorUtility.CopyProxyToUdon(_kinelVideoPlayerUI);
            
            EditorUtility.SetDirty(_kinelVideoPlayerUI);
            // _kinelVideoPlayerUI.UpdateProxy();
        }
        
        
    }
}