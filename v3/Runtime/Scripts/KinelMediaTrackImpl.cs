using System;
using Kinel.VideoPlayer.V3.Udon.System;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase;

namespace Kinel.VideoPlayer.V3.Scripts
{
    // Editor用
    // Runtime用KinelMediaTrackは闇クラスを使っているため、Editorのインスペクタ編集・シーンシリアライズ用にこの型を使用する。
    // Build 時に KinelPlaylistBuildProcessor が並列配列 (urls[]/titles[]/types[]) に展開する。
    [Serializable]
    public class KinelMediaTrackImpl
    {
        public VRCUrl Url;
        public string Title = string.Empty;
        public KinelMediaType Type = KinelMediaType.AvPro;

        public KinelMediaTrackImpl()
        {
            Url = VRCUrl.Empty;
            Title = string.Empty;
            Type = KinelMediaType.AvPro;
        }

        public KinelMediaTrackImpl(VRCUrl url, string title, KinelMediaType type)
        {
            Url = url;
            Title = title;
            Type = type;
        }

#if UNITY_EDITOR && !COMPILER_UDONSHARP
        [CustomPropertyDrawer(typeof(KinelMediaTrackImpl))]
        public class KinelMediaTrackImplPropertyDrawer : PropertyDrawer
        {
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                var rect = new Rect(position.x, position.y + 2, position.width, EditorGUIUtility.singleLineHeight);

                var title = property.FindPropertyRelative(nameof(Title));
                var url = property.FindPropertyRelative(nameof(Url));
                var mode = property.FindPropertyRelative(nameof(Type));

                EditorGUI.PropertyField(rect, title);
                rect.y += EditorGUIUtility.singleLineHeight + 2;
                EditorGUI.PropertyField(rect, url);
                rect.y += EditorGUIUtility.singleLineHeight + 2;
                mode.enumValueIndex = (int)(KinelMediaType)EditorGUI.EnumPopup(rect, "Type",
                    (KinelMediaType)Enum.ToObject(typeof(KinelMediaType), mode.intValue));
            }

            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                return EditorGUI.GetPropertyHeight(property.FindPropertyRelative(nameof(Title)))
                       + EditorGUI.GetPropertyHeight(property.FindPropertyRelative(nameof(Url)))
                       + EditorGUI.GetPropertyHeight(property.FindPropertyRelative(nameof(Type)))
                       + 8;
            }
        }
#endif
    }
}