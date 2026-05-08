using System;
using System.Linq;
using Kinel.VideoPlayer.V3.Editor;
using Kinel.VideoPlayer.V3.Udon.Interface;
using Kinel.VideoPlayer.V3.Udon.System;
using Kinel.VideoPlayer.V3.Udon.System.Component;
using TMPro;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VRC.Udon;
using VRC.Udon.Wrapper.Modules;

namespace Kinel.VideoPlayer.V3.Editor
{
    public class KinelPlaylistBuildProcessor : BaseKinelVideoPlayerEditor, IProcessSceneWithReport
    {
        public const string ParentPrefabGUID = "856f152d9ab34f80ad92e701a034ca7b";
        public const string ContentPrefabGUID = "de68a33a73954ff0b707196856d35ff7";

        public int callbackOrder
        {
            get => 0;
        }

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            var parent = KinelEditorUtility.LoadPrefabByGUID<GameObject>(ParentPrefabGUID);
            var content = KinelEditorUtility.LoadPrefabByGUID<GameObject>(ContentPrefabGUID);

            var playlistDataComponents = scene.GetRootGameObjects()
                .SelectMany(go => go.GetComponentsInChildren<KinelPlaylist>(true));
            var uiComponents = scene.GetRootGameObjects()
                .SelectMany(go => go.GetComponentsInChildren<KinelUIController>(true));


            foreach (var udon in uiComponents)
            {
                var playlistParent = udon.PlaylistUI;

                // Get total number of playlists
                var playlist = udon.Controller.GetComponentInChildren<KinelPlaylist>();
                if (playlist == null)
                {
                    playlist = udon.Controller.transform.parent.GetComponentInChildren<KinelPlaylist>();
                    if (playlist == null)
                    {
                        LogWarning($"Playlist not found on {udon.name}");
                        continue;
                    }
                }

                udon.SetProgramVariable("playlist", playlist);

                var playlistCount = playlist.GetPlaylistCount();

                var listPanelParent = playlistParent.transform.Find("Playlist Selector/Parent/Viewport/Content");

                Debug.Log($"Playlist count: {playlistCount}");
                for (int i = 0; i < playlistCount; i++)
                {
                    Log($"playlist {i}");
                    var tracks = playlist.GetPlaylist(i);
                    var playlistContent = Instantiate(parent, playlistParent.transform);
                    playlistContent.SetActive(false);
                    var trackParent = playlistContent.transform.Find("Parent/Viewport/Content");
                    var udonBehaviour = udon.GetComponent<UdonBehaviour>();


                    foreach (var track in tracks)
                    {
                        var trackObject = Instantiate(content, trackParent);
                        trackObject.name = track.Title();
                        var text = trackObject.GetComponentInChildren<TextMeshProUGUI>();
                        var btn = trackObject.GetComponentInChildren<Button>();

                        text.SetText(track.Title());
                        UnityEventTools.AddStringPersistentListener(
                            btn.onClick,
                            (UnityAction<string>)Delegate.CreateDelegate(
                                typeof(UnityAction<string>),
                                udonBehaviour,
                                "SendCustomEvent"
                            ),
                            nameof(KinelUIController.OnPlaylistTrackSelect)
                        );
                    }

                    var parentSelectItem = Instantiate(content, listPanelParent.transform);

                    // Event注入
                    Log($"Event注入 {i}, {playlist.PlaylistNames[i]}");
                    var button = parentSelectItem.GetComponentInChildren<Button>();
                    var returnButton = playlistContent.transform.Find("Header/Back").GetComponent<Button>();

                    UnityEventTools.AddStringPersistentListener(
                        button.onClick,
                        (UnityAction<string>)Delegate.CreateDelegate(
                            typeof(UnityAction<string>),
                            udonBehaviour,
                            "SendCustomEvent"
                        ),
                        nameof(KinelUIController.OnPlaylistSelect)
                    );

                    UnityEventTools.AddStringPersistentListener(
                        returnButton.onClick,
                        (UnityAction<string>)Delegate.CreateDelegate(
                            typeof(UnityAction<string>),
                            udonBehaviour,
                            "SendCustomEvent"
                        ),
                        nameof(KinelUIController.OnReturnToPlaylistSelect)
                    );

                    var item = parentSelectItem.GetComponentInChildren<TextMeshProUGUI>();
                    item.SetText(playlist.PlaylistNames[i]);
                }
            }
        }

        private void TryBind(Button button, KinelUIController controler, string callbackMethodName)
        {
            UnityEventTools.AddStringPersistentListener(
                button.onClick,
                (UnityAction<string>)Delegate.CreateDelegate(
                    typeof(UnityAction<string>),
                    controler,
                    "SendCustomEvent"
                ),
                callbackMethodName
            );
        }
    }
}