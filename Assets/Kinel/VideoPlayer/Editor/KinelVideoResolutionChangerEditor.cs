using System.Collections.Generic;
using Kinel.VideoPlayer.Editor.Internal;
using Kinel.VideoPlayer.Scripts;
using Kinel.VideoPlayer.Udon;
using Kinel.VideoPlayer.Udon.Module;
using UdonSharpEditor;
using UnityEditor;
using UnityEngine;

namespace Kinel.VideoPlayer.Editor
{
    [CustomEditor(typeof(KinelResolutionChangerScript))]
    public class KinelVideoResolutionChangerEditor : KinelEditorBase
    {
        
        private KinelResolutionChangerScript _resolutionChangerScript;

        private SerializedProperty _kinelVideoPlayer, _animator, _resolutionArray, _text, _isAutoFill;
        private bool _isOpen = false;
        public void OnEnable()
        {
            _resolutionChangerScript = target as KinelResolutionChangerScript;
            _kinelVideoPlayer = serializedObject.FindProperty(nameof(KinelResolutionChangerScript.videoPlayer));
            _animator = serializedObject.FindProperty(nameof(KinelResolutionChangerScript.animator));
            _text = serializedObject.FindProperty(nameof(KinelResolutionChangerScript.text));
            _resolutionArray = serializedObject.FindProperty(nameof(KinelResolutionChangerScript.resolutionArray));
            _isAutoFill = serializedObject.FindProperty(nameof(KinelResolutionChangerScript.isAutoFill));
        }
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();

            OnInternalInspector();
            
            if (serializedObject.ApplyModifiedProperties())
            {
                ApplyUdonProperties();
            }
        }

        private void OnInternalInspector()
        {
            EditorGUILayout.PropertyField(_kinelVideoPlayer);
            EditorGUILayout.PropertyField(_animator);
            AutoFillProperties();
            if (_isAutoFill.boolValue)
            {
                EditorGUILayout.HelpBox("自動的に設定されました。", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("複数のビデオプレイヤーが存在するときは、`KineLVideoPlayer/KineLVP System/`を指定してあげてください。", MessageType.Info);
            }
            
            _isOpen = EditorGUILayout.Foldout(_isOpen, "Internal / 内部用");
            if (_isOpen)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                {
                    EditorGUILayout.LabelField("----- internal -----");
                    EditorGUI.indentLevel++;
                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(_text);
                    EditorGUILayout.PropertyField(_resolutionArray);
                    EditorGUILayout.Space();
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.EndVertical();
            }
        }
        
        public override void ApplyUdonProperties()
        {
            var udon = _resolutionChangerScript.GetUdonSharpComponentInChildren<VideoResolutionModule>(true);
            if (udon == null)
            {
                Debug.Log($"Udon is null");
                return;
            }

            var resolutionArray = new List<int>();

            for (int i = 0; i < _resolutionArray.arraySize; i++)
            {
                resolutionArray.Add(_resolutionArray.GetArrayElementAtIndex(i).intValue);
            }
            
            Undo.RecordObject(udon, "Applay Udon properties.");
            udon.UpdateProxy();
            udon.SetProgramVariable("animator", _animator.objectReferenceValue);
            udon.SetProgramVariable("resolutionText", _text.objectReferenceValue);
            udon.SetProgramVariable("resolutionArray", resolutionArray.ToArray());
            udon.ApplyProxyModifications();
            UdonSharpEditorUtility.CopyProxyToUdon(udon);
            EditorUtility.SetDirty(udon);
        }
        
        private void AutoFillProperties()
        {
            if (_kinelVideoPlayer.objectReferenceValue == null)
            {
                if (_resolutionChangerScript.transform.parent == null)
                    return;
                
                Undo.RecordObject(_resolutionChangerScript, "Instance attached");
                KinelEditorUtilities.FillUdonSharpInstance<KinelVideoPlayer>(ref _kinelVideoPlayer,
                    _resolutionChangerScript.transform.parent.gameObject, false);
                EditorUtility.SetDirty(_resolutionChangerScript);
            }
            if (_animator.objectReferenceValue == null)
            {
                if (_kinelVideoPlayer.objectReferenceValue == null)
                    return;

                var animators = ((KinelVideoPlayer) _kinelVideoPlayer.objectReferenceValue).GetComponents<Animator>();
                if (animators.Length >= 2 || animators.Length == 0)
                    return;

                _isAutoFill.boolValue = true;
                _animator.objectReferenceValue = animators[0];
                EditorUtility.SetDirty(_resolutionChangerScript);
            }
            
        }
        
    }
}