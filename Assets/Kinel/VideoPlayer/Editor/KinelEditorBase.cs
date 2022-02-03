

using Kinel.VideoPlayer.Scripts;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Kinel.VideoPlayer.Editor
{
    public class KinelEditorBase : UnityEditor.Editor
    {

        internal const string DEBUG_LOG_PREFIX = "[<color=#58ACFA>KineL</color>]";
        internal const string DEBUG_ERROR_PREFIX = "[<color=#dc143c>KineL</color>]";

        internal const string HEADER_IMAGE_GUID = "202a1853611bcb84886fca2cfb43c0aa";
        
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
            }
            
            if(headerTexture != null){
                var rect = new Rect(EditorGUIUtility.currentViewWidth, 5, headerTexture.width * 0.4f, headerTexture.height * 0.4f);
                GUI.DrawTexture(rect, headerTexture, ScaleMode.StretchToFill);
            }
            
            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                EditorGUILayout.LabelField("Kinel Video Player", EditorStyles.boldLabel);
                
                EditorGUILayout.Space();
            }
            EditorGUILayout.EndVertical();
            
        }

        public KinelVideoPlayerScript[] GetVideoPlayers()
        {
            return FindObjectsOfType<KinelVideoPlayerScript>();
        }
        
        
    }
}