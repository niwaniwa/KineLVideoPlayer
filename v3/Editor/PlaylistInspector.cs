using Kinel.VideoPlayer.V3.Editor.UI;
using Kinel.VideoPlayer.V3.Scripts.VideoPlayer;
using UnityEditor;
using UnityEngine;
using Kinel.VideoPlayer.V3.Udon.System.Component;

[CustomEditor(typeof(KinelPlaylist))]
public class PlaylistInspector : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        GUILayout.Space(8);

        DrawDefaultInspector();

        if (GUILayout.Button("Open Playlist Editor"))
        {
            var udon = (KinelPlaylist)target;
            var script = udon != null ? udon.GetComponent<KinelPlaylistScript>() : null;
            if (script != null)
            {
                PlaylistEditorWindow.Open(script);
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "KineL Playlist Editor",
                    "同一 GameObject に KinelPlaylistScript が見つかりません。proxy script を追加してください。",
                    "OK");
            }
        }
    }
}