

using Kinel.VideoPlayer.Scripts;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Kinel.VideoPlayer.Editor
{
    public abstract class KinelEditorBase : UnityEditor.Editor
    {

        internal const string DEBUG_LOG_PREFIX = "[<color=#58ACFA>KineL</color>]";
        internal const string DEBUG_ERROR_PREFIX = "[<color=#dc143c>KineL</color>]";

        internal const string HEADER_IMAGE_GUID = "6bc2959ee80eb4d4dbdb46be56f94dfa";
        
        private Texture headerTexture;
        
        public override void OnInspectorGUI()
        {
            EditorGUILayout.Space();
            Header();
            EditorGUILayout.Space();
        }

        private void Header()
        {
            if (headerTexture == null)
            {
                headerTexture = AssetDatabase.LoadAssetAtPath<Texture>(AssetDatabase.GUIDToAssetPath(HEADER_IMAGE_GUID));
                // Debug.Log($"{headerTexture == null}");
            }
            
            if(headerTexture != null){
                Rect rect = new Rect();
                rect.width = EditorGUIUtility.currentViewWidth - (EditorGUIUtility.currentViewWidth * 0.1f);
                rect.height = rect.width / 4f; // yoko : 975, tate 250, hiritu = 3.9
                Rect yoyaku = GUILayoutUtility.GetRect(rect.width, rect.height); // GetRect参照しろ！！！！
                rect.x = EditorGUIUtility.currentViewWidth * 0.05f;
                rect.y = yoyaku.y;
                GUI.DrawTexture(rect, headerTexture, ScaleMode.StretchToFill);
            }
            
            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Kinel Video Player", EditorStyles.boldLabel);
                EditorGUILayout.Space();
            }
            EditorGUILayout.EndVertical();
            
        }

        public KinelVideoPlayerScript[] GetVideoPlayers()
        {
            return FindObjectsOfType<KinelVideoPlayerScript>();
        }

        public abstract void ApplyUdonProperties();


    }
}