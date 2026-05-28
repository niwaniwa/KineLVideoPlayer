using Kinel.VideoPlayer.V3.Udon.Interface;
using UnityEditor;
using UnityEngine;

namespace Kinel.VideoPlayer.V3.Editor
{
    [CustomEditor(typeof(KinelButtonHoverRelay))]
    public class KinelButtonHoverRelayEditor : UnityEditor.Editor
    {
        private SerializedProperty _tooltipOffsetProp;

        private void OnEnable()
        {
            _tooltipOffsetProp = serializedObject.FindProperty("tooltipOffset");
        }

        private void OnSceneGUI()
        {
            var relay = (KinelButtonHoverRelay)target;
            var worldPos = relay.transform.position + _tooltipOffsetProp.vector3Value;

            EditorGUI.BeginChangeCheck();
            var newWorldPos = Handles.PositionHandle(worldPos, Quaternion.identity);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(relay, "Move Tooltip Offset");
                _tooltipOffsetProp.vector3Value = newWorldPos - relay.transform.position;
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
