using System;
using Kinel.VideoPlayer.Scripts;
using Kinel.VideoPlayer.Udon;
using UdonSharpEditor;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase;

namespace Kinel.VideoPlayer.Editor
{
    [CustomEditor(typeof(KinelVideoPlayerScript))]
    public class KinelVideoPlayerEditor : KinelEditorBase
    {
        
        private KinelVideoPlayerScript _videoPlayer;

        private SerializedProperty _kinelVideoPlayer, _loop, _isAutoFill, _retryLimit, _deleyLimit, _enableErrorRetry, _enableDefaultUrl, _defaultUrl, _defaultUrlMode;

        public void OnEnable()
        {
            _videoPlayer = target as KinelVideoPlayerScript;
            _kinelVideoPlayer = serializedObject.FindProperty(nameof(KinelVideoPlayerScript.videoPlayer));
            _loop = serializedObject.FindProperty(nameof(KinelVideoPlayerScript.loop));
            _isAutoFill = serializedObject.FindProperty(nameof(KinelVideoPlayerScript.isAutoFill));
            _retryLimit = serializedObject.FindProperty(nameof(KinelVideoPlayerScript.retryLimit));
            _deleyLimit = serializedObject.FindProperty(nameof(KinelVideoPlayerScript.deleyLimit));
            _enableErrorRetry = serializedObject.FindProperty(nameof(KinelVideoPlayerScript.enableErrorRetry));
            _enableDefaultUrl = serializedObject.FindProperty(nameof(KinelVideoPlayerScript.enableDefaultUrl));
            _defaultUrl = serializedObject.FindProperty(nameof(KinelVideoPlayerScript.defaultUrl));
            _defaultUrlMode = serializedObject.FindProperty(nameof(KinelVideoPlayerScript.defaultUrlMode));
            
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            serializedObject.Update();

            CheckSystemDatas();
            
            EditorGUILayout.Space();

            PlayerSettingsInspector();
            
            EditorGUILayout.Space();
            
            base.DrawFooter();

            if (serializedObject.ApplyModifiedProperties())
            {
                ApplyUdonProperties();
            }
            
        }
        
        public void PlayerSettingsInspector()
        {
            // playlist settings
            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                EditorGUILayout.LabelField("設定");
                EditorGUI.indentLevel++;
                EditorGUILayout.Space();
                // loop
                EditorGUILayout.PropertyField(_loop, new GUIContent("ループ / loop"));
                EditorGUILayout.PropertyField(_enableDefaultUrl, new GUIContent("デフォルト再生"));
                EditorGUILayout.HelpBox("Join時に自動再生します。プレイリストなどがある場合はそちらが優先されます。", MessageType.Info);
                if (_enableDefaultUrl.boolValue)
                {
                    EditorGUI.indentLevel++;
                    // var text = EditorGUILayout.TextField("再生URL");
                    // defaultUrl = new VRCUrl(text);
                    EditorGUILayout.PropertyField(_defaultUrl, new GUIContent("再生URL"));
                    _defaultUrlMode.intValue = (int) (PlayerMode) EditorGUILayout.EnumPopup(new GUIContent("Url Type"), (PlayerMode) Enum.ToObject(typeof(PlayerMode), _defaultUrlMode.intValue));
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.Space();
                
                EditorGUILayout.LabelField("同期関係 / Sync");
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_deleyLimit, new GUIContent("遅延許容時間(ミリ秒)"));
                EditorGUILayout.PropertyField(_enableErrorRetry, new GUIContent("エラー時自動再読み込み"));
                if (_enableErrorRetry.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(_retryLimit, new GUIContent("最大回数"));
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.Space();
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();
        }
        

        public override void ApplyUdonProperties()
        {
            var videoPlayer = _videoPlayer.GetUdonSharpComponentInChildren<KinelVideoPlayer>(true);
            if (videoPlayer == null) return;
            
            Undo.RecordObject(videoPlayer, "Applay videoplayer properties.");
            videoPlayer.UpdateProxy(); // Deleted in the near future: udon update
            videoPlayer.SetProgramVariable("deleyLimit", _deleyLimit.floatValue);
            videoPlayer.SetProgramVariable("retryLimit", _retryLimit.intValue);
            videoPlayer.SetProgramVariable("enableErrorRetry", _enableErrorRetry.boolValue);
            videoPlayer.SetProgramVariable("defaultLoop", _loop.boolValue);
            videoPlayer.SetProgramVariable("enableDefaultUrl", _enableDefaultUrl.boolValue);
            videoPlayer.SetProgramVariable("defaultPlayUrl", new VRCUrl(_defaultUrl.stringValue));
            videoPlayer.SetProgramVariable("defaultPlayUrlMode", _defaultUrlMode.intValue);
            videoPlayer.ApplyProxyModifications(); // Deleted in the near future: udon update
            UdonSharpEditorUtility.CopyProxyToUdon(videoPlayer);
            EditorUtility.SetDirty(videoPlayer);
        }
        
        public void CheckSystemDatas()
        {
            // var editor = _videoPlayer.GetUdonSharpComponentsInChildren<KinelVideoPlayerUI>();
            // Debug.Log(editor.Length);
        }



    }
}