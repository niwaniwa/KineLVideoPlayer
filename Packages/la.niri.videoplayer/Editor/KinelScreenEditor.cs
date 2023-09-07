using System;
using System.Reflection;
using Kinel.VideoPlayer.Editor.Internal;
using Kinel.VideoPlayer.Scripts;
using Kinel.VideoPlayer.Udon;
using Kinel.VideoPlayer.Udon.Module;
using UdonSharpEditor;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Video.Components.AVPro;

namespace Kinel.VideoPlayer.Editor
{
    [CustomEditor(typeof(KinelScreenScript))]
    public class KinelScreenEditor : KinelEditorBase
    {

        private KinelScreenScript _screenModule;

        private SerializedProperty _kinelVideoPlayer, _screenName, _isAutoFill, _propertyName, _mirrorInverion, _transparency, _backCulling, _materialIndex;


        public void OnEnable()
        {
            _screenModule = target as KinelScreenScript;
            _kinelVideoPlayer = serializedObject.FindProperty(nameof(KinelScreenScript.videoPlayer));
            _screenName = serializedObject.FindProperty(nameof(KinelScreenScript.screenName));
            _isAutoFill = serializedObject.FindProperty(nameof(KinelScreenScript.isAutoFill));
            _propertyName = serializedObject.FindProperty(nameof(KinelScreenScript.propertyName));
            _mirrorInverion = serializedObject.FindProperty(nameof(KinelScreenScript.mirrorInverion));
            _transparency = serializedObject.FindProperty(nameof(KinelScreenScript.transparency));
            _backCulling = serializedObject.FindProperty(nameof(KinelScreenScript.backCulling));
            _materialIndex = serializedObject.FindProperty(nameof(KinelScreenScript.materialIndex));
            
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();
            
            // playlist header
            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                EditorGUILayout.LabelField("KineL Video Screen");
                
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("基本設定");
                EditorGUI.indentLevel++;
                EditorGUILayout.Space();
                _isAutoFill.enumValueIndex = (int) KinelEditorUtilities.FillUdonSharpInstance<KinelVideoPlayer>(ref _kinelVideoPlayer, _screenModule.gameObject, false);
                if (_kinelVideoPlayer.objectReferenceValue)
                    EditorGUILayout.LabelField(_kinelVideoPlayer.displayName, "自動設定されました。");
                else
                    EditorGUILayout.PropertyField(_kinelVideoPlayer);
                
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

        public override void ApplyUdonProperties()
        {
            var screenUdon = _screenModule.GetComponent<KinelScreenModule>();
            if (screenUdon == null)
            {
                Debug.LogError($"{DEBUG_ERROR_PREFIX} screenUdon null");
                return;
            }
            Undo.RecordObject(screenUdon, "Instance attached");
            screenUdon.SetProgramVariable("propertyName", _propertyName.stringValue);
            screenUdon.SetProgramVariable("mirrorInverion", _mirrorInverion.boolValue);
            screenUdon.SetProgramVariable("transparency", _transparency.floatValue);

            UdonSharpEditorUtility.CopyProxyToUdon(screenUdon);
            EditorUtility.SetDirty(screenUdon);
        }
    }
}