using System;
using UnityEditor;
using UnityEngine;

namespace Kinel.VideoPlayer.Scripts
{
    [Serializable]
    public class VideoData
    {
        public string title;

        public string description;

        public string url;

        public int mode;

        
#if UNITY_EDITOR
        [CustomPropertyDrawer(typeof(VideoData))]
        public class VideoDataPropertyDrawer : PropertyDrawer
        {
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                var rect = new Rect(position.x, position.y  + 2, position.width, EditorGUIUtility.singleLineHeight);
                
                var title = property.FindPropertyRelative(nameof(VideoData.title));
                var url = property.FindPropertyRelative(nameof(VideoData.url));
                var mode = property.FindPropertyRelative(nameof(VideoData.mode));

                EditorGUI.PropertyField(rect, title);
                rect.y += EditorGUIUtility.singleLineHeight + 2;
                EditorGUI.PropertyField(rect, url);
                rect.y += EditorGUIUtility.singleLineHeight + 2;
                // EditorGUI.PropertyField(rect, mode);
                // rect.y += EditorGUIUtility.singleLineHeight + 2;
                mode.intValue = (int) (PlayerMode) EditorGUI.EnumPopup(rect, "hoge", (PlayerMode) Enum.ToObject(typeof(PlayerMode), mode.intValue));
            }

            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                return EditorGUI.GetPropertyHeight(property.FindPropertyRelative(nameof(VideoData.title)))
                       + EditorGUI.GetPropertyHeight(property.FindPropertyRelative(nameof(VideoData.url)))
                       + EditorGUI.GetPropertyHeight(property.FindPropertyRelative(nameof(VideoData.mode)))
                       + 8;
            }
            
        }
#endif
    }
}