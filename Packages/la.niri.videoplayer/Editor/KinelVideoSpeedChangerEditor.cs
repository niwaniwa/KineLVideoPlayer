using Kinel.VideoPlayer.Editor.Internal;
using Kinel.VideoPlayer.Scripts;
using Kinel.VideoPlayer.Scripts.Parameter;
using Kinel.VideoPlayer.Udon;
using Kinel.VideoPlayer.Udon.Module;
using UdonSharpEditor;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase;

namespace Kinel.VideoPlayer.Editor
{
    [CustomEditor(typeof(KinelSpeedChangerScript))]
    public class KinelVideoSpeedChangerEditor : KinelEditorBase
    {
        
        private KinelSpeedChangerScript _speedChangerScript;

        private SerializedProperty _kinelVideoPlayer, _animator, _speedChangerSlider, _text, _min, _max, _increaseSpeed, _audioSource, _pitchChange, _isAutoFill, _manual, _animationParameterMax;
        private bool _isOpen = false;
        public void OnEnable()
        {
            _speedChangerScript = target as KinelSpeedChangerScript;
            _kinelVideoPlayer = serializedObject.FindProperty(nameof(KinelSpeedChangerScript.videoPlayer));
            _animator = serializedObject.FindProperty(nameof(KinelSpeedChangerScript.animator));
            _speedChangerSlider = serializedObject.FindProperty(nameof(KinelSpeedChangerScript.speedChangerSlider));
            _text = serializedObject.FindProperty(nameof(KinelSpeedChangerScript.text));
            _min = serializedObject.FindProperty(nameof(KinelSpeedChangerScript.min));
            _max = serializedObject.FindProperty(nameof(KinelSpeedChangerScript.max));
            _animationParameterMax = serializedObject.FindProperty(nameof(KinelSpeedChangerScript.animationParameterMax));
            _increaseSpeed = serializedObject.FindProperty(nameof(KinelSpeedChangerScript.increaseSpeed));
            _audioSource = serializedObject.FindProperty(nameof(KinelSpeedChangerScript.source));
            _pitchChange = serializedObject.FindProperty(nameof(KinelSpeedChangerScript.pitchChange));
            _isAutoFill = serializedObject.FindProperty(nameof(KinelSpeedChangerScript.isAutoFill));
            _manual = serializedObject.FindProperty(nameof(KinelSpeedChangerScript.manual));
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();
            
;            
            OnInternalInspector();
            
            if (serializedObject.ApplyModifiedProperties())
            {
                ApplyUdonProperties();
            }
        }

        private void OnInternalInspector()
        {
            AutoFillProperties();
            _isAutoFill.enumValueIndex = (int) KinelEditorUtilities.FillUdonSharpInstance<KinelVideoPlayer>(ref _kinelVideoPlayer, _speedChangerScript.gameObject, false);
            if (_kinelVideoPlayer.objectReferenceValue)
                EditorGUILayout.LabelField(_kinelVideoPlayer.displayName, "自動設定されました。");
            else
                EditorGUILayout.PropertyField(_kinelVideoPlayer);

            EditorGUILayout.PropertyField(_animator);
            
            _isOpen = EditorGUILayout.Foldout(_isOpen, "Internal / 内部用");
            if (_isOpen)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                {
                    EditorGUILayout.LabelField("----- internal -----");
                    EditorGUI.indentLevel++;
                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(_speedChangerSlider);
                    EditorGUILayout.PropertyField(_text);
                    EditorGUILayout.PropertyField(_min);
                    EditorGUILayout.PropertyField(_max);
                    EditorGUILayout.PropertyField(_animationParameterMax);
                    EditorGUILayout.PropertyField(_increaseSpeed);
                    EditorGUILayout.PropertyField(_audioSource);
                    EditorGUILayout.PropertyField(_pitchChange);
                    EditorGUILayout.Space();
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.EndVertical();
            }
        }

        public override void ApplyUdonProperties()
        {
            var udon = _speedChangerScript.GetUdonSharpComponentInChildren<VideoSpeedChangeModule>(true);
            if (udon == null)
            {
                Debug.Log($"Udon is null");
                return;
            }
            
            Undo.RecordObject(udon, "Applay Udon properties.");
            udon.UpdateProxy();
            udon.SetProgramVariable("videoPlayer", _kinelVideoPlayer.objectReferenceValue);
            udon.SetProgramVariable("animator", _animator.objectReferenceValue);
            udon.SetProgramVariable("speedChangerSlider", _speedChangerSlider.objectReferenceValue);
            udon.SetProgramVariable("text", _text.objectReferenceValue);
            udon.SetProgramVariable("min", _min.floatValue);
            udon.SetProgramVariable("max", _max.floatValue);
            udon.SetProgramVariable("animationParameterMax", _animationParameterMax.floatValue);
            udon.SetProgramVariable("increaseSpeed", _increaseSpeed.floatValue);
            udon.SetProgramVariable("source", _audioSource.objectReferenceValue);
            udon.SetProgramVariable("pitchChange", _pitchChange.boolValue);
            udon.ApplyProxyModifications();
            UdonSharpEditorUtility.CopyProxyToUdon(udon);
            EditorUtility.SetDirty(udon);
        }
        
        private void AutoFillProperties()
        {
            serializedObject.Update();
            if (_animator.objectReferenceValue == null)
            {
               
                if (_kinelVideoPlayer.objectReferenceValue == null)
                    return;

                var animators = ((KinelVideoPlayer) _kinelVideoPlayer.objectReferenceValue).GetComponents<Animator>();
                if (animators.Length >= 2 || animators.Length == 0)
                    return;
                
                _animator.objectReferenceValue = animators[0];

            }
            serializedObject.Update();
        }
    }
}