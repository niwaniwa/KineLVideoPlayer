using System;
using System.Reflection;
using Kinel.VideoPlayer.Udon;
using Kinel.VideoPlayer.Udon.Module;
using UdonSharpEditor;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Video.Components.AVPro;

namespace Kinel.VideoPlayer.Editor
{
    [CustomEditor(typeof(KinelScreenModule))]
    public class KinelScreenEditor : KinelEditorBase
    {

        private KinelScreenModule _screenModule;

        private SerializedProperty _kinelVideoPlayer, _screenName, _isAutoFill, _propertyName, _mirrorInverion, _transparency, _backCulling, _materialIndex;


        public void OnEnable()
        {
            _screenModule = target as KinelScreenModule;
            _kinelVideoPlayer = serializedObject.FindProperty(nameof(KinelScreenModule.videoPlayer));
            _screenName = serializedObject.FindProperty(nameof(KinelScreenModule.screenName));
            _isAutoFill = serializedObject.FindProperty(nameof(KinelScreenModule.isAutoFill));
            _propertyName = serializedObject.FindProperty(nameof(KinelScreenModule.propertyName));
            _mirrorInverion = serializedObject.FindProperty(nameof(KinelScreenModule.mirrorInverion));
            _transparency = serializedObject.FindProperty(nameof(KinelScreenModule.transparency));
            _backCulling = serializedObject.FindProperty(nameof(KinelScreenModule.backCulling));
            _materialIndex = serializedObject.FindProperty(nameof(KinelScreenModule.materialIndex));
            
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) 
                return;

            serializedObject.Update();
            

            // playlist header
            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                EditorGUILayout.LabelField("KineL Video Screen");
                
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("基本設定");
                EditorGUI.indentLevel++;
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(_kinelVideoPlayer);
                AutoFillProperties();
                if (_isAutoFill.boolValue)
                    EditorGUILayout.HelpBox("自動的に設定されました。", MessageType.Info);
                else if (_kinelVideoPlayer.objectReferenceValue == null)
                    EditorGUILayout.HelpBox("複数のビデオプレイヤーが存在するか、ビデオプレイヤーが存在していません。" +
                                            "", MessageType.Warning);
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
                
                EditorGUILayout.LabelField("Screen settings");
                EditorGUI.indentLevel++;
                
                EditorGUILayout.PropertyField(_mirrorInverion, new GUIContent("ミラー反転"));
                // _transparency.floatValue = EditorGUILayout.Slider("透明度", _transparency.floatValue, 0, 1f);
                EditorGUILayout.PropertyField(_backCulling, new GUIContent("裏面表示"));
                
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("----- Internal / 内部用");
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_screenName);
                EditorGUILayout.PropertyField(_propertyName);
                EditorGUILayout.PropertyField(_materialIndex);
                

                
                EditorGUILayout.Space();
                
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            if (serializedObject.ApplyModifiedProperties())
            {
                ApplyUdonProperties();
            }

        }


        private void AutoFillProperties()
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
                    _kinelVideoPlayer.objectReferenceValue = system[0];
                    
                    
                }

            }
            
        }

        public override void ApplyUdonProperties()
        {
            var screen = target as KinelScreenModule;
            Undo.RecordObject(screen, "Instance attached");
            screen.SetProgramVariable("propertyName", _propertyName.stringValue);
            screen.SetProgramVariable("mirrorInverion", _mirrorInverion.boolValue);
            screen.SetProgramVariable("transparency", _transparency.floatValue);

            UdonSharpEditorUtility.CopyProxyToUdon(_screenModule);
            EditorUtility.SetDirty(screen);
        }
    }
}