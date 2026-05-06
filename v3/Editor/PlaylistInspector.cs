using System.Collections.Generic;
using Kinel.VideoPlayer.V3.Scripts;
using UnityEditor;
using UnityEngine;
using Kinel.VideoPlayer.V3.Udon.System.Component; // KinelPlaylist

[CustomEditor(typeof(KinelPlaylist))]
public class PlaylistInspector : UnityEditor.Editor
{
    // ボタンを押したあとに表示するためのキャッシュ
    private bool _showContents = false;
    private List<KinelMediaTrackImpl[]> _cachedPlaylists;

    public override void OnInspectorGUI()
    {
        // まず通常のフィールドを描画
        DrawDefaultInspector();

        GUILayout.Space(8);

        // 「Show Playlist Contents」ボタン
        if (GUILayout.Button("Show Playlist Contents"))
        {
            FetchAndCachePlaylists();
            _showContents = true;
        }

        // キャッシュがあれば中身を描画
        if (_showContents && _cachedPlaylists != null)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("── Playlist Contents ──", EditorStyles.boldLabel);

            for (int i = 0; i < _cachedPlaylists.Count; i++)
            {
                var tracks = _cachedPlaylists[i];
                EditorGUILayout.LabelField($"Playlist #{i}:", EditorStyles.label);
                EditorGUI.indentLevel++;

                if (tracks == null || tracks.Length == 0)
                {
                    EditorGUILayout.LabelField(" (empty)", EditorStyles.miniLabel);
                }
                else
                {
                    for (int j = 0; j < tracks.Length; j++)
                    {
                        var t = tracks[j];
                        if (t != null)
                        {
                            EditorGUILayout.LabelField(
                                $"{j}: {t.Title}  ({t.Url})",
                                EditorStyles.miniLabel
                            );
                        }
                        else
                        {
                            EditorGUILayout.LabelField(
                                $"{j}: <null>",
                                EditorStyles.miniLabel
                            );
                        }
                    }
                }

                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
            }
        }
    }

    private void FetchAndCachePlaylists()
    {
    }
}