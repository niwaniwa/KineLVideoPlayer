using Kinel.VideoPlayer.Scripts;
using Kinel.VideoPlayer.Udon.Controller;
using Kinel.VideoPlayer.Udon.Module;
using UdonSharpEditor;
using UnityEditor;
using UnityEngine;

namespace Kinel.VideoPlayer.Editor
{
    [CustomEditor(typeof(KinelLocalSettingControllerScript))]
    public class KinelLocalSettingControllerEditor : KinelEditorBase
    {
        
        private KinelLocalSettingControllerScript localSettingController;

        private SerializedProperty _mirrorInversion;

        public void OnEnable()
        {
            localSettingController = target as KinelLocalSettingControllerScript;
            _mirrorInversion = serializedObject.FindProperty(nameof(KinelLocalSettingControllerScript.mirrorInversion));
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();
            
            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                EditorGUILayout.LabelField("KineL Local Setting Controller");
                
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("基本設定");
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Screen settings");
                EditorGUI.indentLevel++;
                
                EditorGUILayout.PropertyField(_mirrorInversion, new GUIContent("ミラー反転"));
                EditorGUILayout.HelpBox("ここで設定した値が全てのスクリーンに反映されます。", MessageType.Info);
                
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
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
            var settingController = localSettingController.GetComponentInChildren<KinelLocalSettingController>();
            if (settingController == null)
            {
                Debug.LogError($"{DEBUG_ERROR_PREFIX} setting Controller is null");
                return;
            }
            Undo.RecordObject(settingController, "Instance attached");
            settingController.SetProgramVariable("mirrorInversion", _mirrorInversion.boolValue);

            UdonSharpEditorUtility.CopyProxyToUdon(settingController);
            EditorUtility.SetDirty(settingController);
        }
    }
}