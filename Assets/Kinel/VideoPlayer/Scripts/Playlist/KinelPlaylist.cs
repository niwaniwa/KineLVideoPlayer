

using UdonSharp;
using UnityEngine;
using UnityEngine.UI;

using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;
#if UNITY_EDITOR && !COMPILER_UDONSHARP// Editor拡張用
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UdonSharpEditor;
#endif
namespace Kinel.VideoPlayer.Scripts.Playlist
{
    
#if UNITY_EDITOR && !COMPILER_UDONSHARP
    [Serializable]
#endif
    public class KinelPlaylist : UdonSharpBehaviour
    {
        public KinelVideoScript videoPlayer;
        
        public string[] dummy = new string[0];
        
        public string[] titles = new string[0];
        public string[] descriptions = new string[0];
        public string[] urlString = new string[0];
        public VRCUrl[] urls = new VRCUrl[0];
        public int[] playMode = new int[0];
        public bool autoPlay;

        public GameObject videoPrefab;
        public RectTransform content;

        public GameObject warningUI;

        private bool ready = false, loop = false;
        private int index = 0;
        
        public void Start()
        {
            if (!videoPlayer)
            {
                Debug.LogError("[Kinel] Playlist init error. Video player is null.");
                warningUI.transform.GetChild(0).GetComponent<Text>().text = "...Disabled...";
                warningUI.SetActive(true);
                videoPrefab.SetActive(false);
                return;
            }

            if (urls.Length == 0)
            {
                Debug.LogError("[Kinel] URL is empty");
                warningUI.transform.GetChild(0).GetComponent<Text>().text = "...Disabled...";
                warningUI.SetActive(true);
                videoPrefab.SetActive(false);
                return;
            }
            
            Init();
            videoPlayer.SetList(this);
        }

        public void OnDisable()
        {
            AutoPlayDisable();
        }

        public void PlayVideoByNumber()
        {
            Networking.SetOwner(Networking.LocalPlayer, videoPlayer.gameObject);
            var num = GetSelectedItem();
            PlayVideo(num);
            if(autoPlay)
                SendCustomNetworkEvent(NetworkEventTarget.All, nameof(AutoPlayDisable));
            
        }

        public void PlayVideo(int index)
        {
            videoPlayer.SetGlobalPlayMode(playMode[index]);
            videoPlayer.GetModeChangeInstance().ChangeMode(playMode[index]);
            videoPlayer.PlayVideo(urls[index]);
        }

        public void PlayNextVideo()
        {
            if (!Networking.LocalPlayer.isMaster)
                return;
            
            if (!Networking.IsOwner(Networking.LocalPlayer, videoPlayer.gameObject))
                Networking.SetOwner(Networking.LocalPlayer, videoPlayer.gameObject);

            PlayVideo(index);
            index++;
            if (index >= urls.Length)
                index = 0;
        }
        
        
        public void Init()
        {
            Debug.Log("[Kinel] Playlist Init");
            for (var i = 0; i < urls.Length; i++)
            {
                GameObject prefab = VRCInstantiate(videoPrefab);

                prefab.name = $"Video ({i})";
                prefab.transform.SetParent(content);
                prefab.transform.localPosition = Vector3.zero;
                prefab.transform.localRotation = Quaternion.identity;
                prefab.transform.localScale    = Vector3.one;

                var descriptionComponent = prefab.transform.GetChild(1).GetChild(0).GetComponent<Text>();
                descriptionComponent.text = descriptionComponent.text.Replace("$NAME$", titles[i])
                    .Replace("$DESCRIPTION$", descriptions[i]);
            }

            videoPrefab.SetActive(false);
            
        }

        public int GetSelectedItem()
        {
            var select = 0;
            for (int i = 0; i < content.transform.childCount; i++)
            {
                var button = content.GetChild(i).GetComponent<Button>();
                if (!button.interactable){
                    select = i;
                    button.interactable = true;
                    break;
                }
            }

            return select;
        }

        public void FixedUpdate()
        {
            if (!videoPlayer)
                return;
            
            if (!ready)
                return;
            
            if (!warningUI)
                return;

            if (urls.Length <= 0)
                return;

            if (Networking.LocalPlayer.isMaster && warningUI.activeSelf)
                warningUI.SetActive(false);

            if (videoPlayer.masterOnly && !Networking.LocalPlayer.isMaster)
                warningUI.SetActive(true);
                
            if (!videoPlayer.masterOnly && !Networking.LocalPlayer.isMaster)
                warningUI.SetActive(false);
                
            
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if(player == Networking.LocalPlayer)
                ready = true;

            if (player == Networking.LocalPlayer && autoPlay)
            {
                AutoPlayEnable();
                if (Networking.IsMaster)
                {
                    PlayNextVideo();
                }
            }
                
        }

        public void OnEnable()
        {
            if (!videoPlayer || !videoPlayer.GetVideoPlayer())
                return;
            
        }

        public void AutoPlayEnable()
        {
            autoPlay = true;
            var instance = videoPlayer.GetVideoPlayer();
            if (instance == null)
                return;
            
            loop = instance.Loop;
            instance.Loop = false;
        }
        
        public void AutoPlayDisable()
        {
            Debug.Log($"Disabled Autoplay");
            autoPlay = false;
            videoPlayer.GetVideoPlayer().Loop = (loop != videoPlayer.IsLoop() ? videoPlayer.IsLoop() : loop);
        }

    }

#if UNITY_EDITOR && !COMPILER_UDONSHARP
    [CustomEditor(typeof(KinelPlaylist))]
    internal class KinelPlaylistEditor : Editor
    {
        private bool showReference = false;
        
        private KinelPlaylist kinelPlaylist;
        private ReorderableList list;

        private SerializedProperty kinelVideoPlayer;
        private SerializedProperty content;
        private SerializedProperty videoPrefab;

        private SerializedProperty urlString;
        private SerializedProperty warningUI;
        private SerializedProperty autoPlay;

        private void OnEnable()
        {
            kinelPlaylist = target as KinelPlaylist;

            kinelVideoPlayer = serializedObject.FindProperty(nameof(KinelPlaylist.videoPlayer));
            content = serializedObject.FindProperty(nameof(KinelPlaylist.content));
            videoPrefab = serializedObject.FindProperty(nameof(KinelPlaylist.videoPrefab));
            urlString = serializedObject.FindProperty(nameof(KinelPlaylist.urlString));
            warningUI = serializedObject.FindProperty(nameof(KinelPlaylist.warningUI));
            autoPlay = serializedObject.FindProperty(nameof(KinelPlaylist.autoPlay));

            var dummyList = serializedObject.FindProperty(nameof(KinelPlaylist.dummy));
            var titles = serializedObject.FindProperty(nameof(KinelPlaylist.titles));
            var descriptions = serializedObject.FindProperty(nameof(KinelPlaylist.descriptions));
            var playMode = serializedObject.FindProperty(nameof(KinelPlaylist.playMode));

            list = new ReorderableList(serializedObject, dummyList)
            {
                onAddCallback = reorderableList =>
                {
                    dummyList.arraySize++;
                    titles.arraySize++;
                    urlString.arraySize++;
                    descriptions.arraySize++;
                    playMode.arraySize++;

                    var index = dummyList.arraySize - 1;
                    titles.GetArrayElementAtIndex(index).stringValue = String.Empty;
                    descriptions.GetArrayElementAtIndex(index).stringValue = String.Empty;
                    urlString.GetArrayElementAtIndex(index).stringValue = String.Empty;
                    playMode.GetArrayElementAtIndex(index).intValue = 0;

                    reorderableList.index = index;
                },
                onRemoveCallback = reorderableList =>
                {
                    dummyList.arraySize--;
                    titles.arraySize--;
                    urlString.arraySize--;
                    descriptions.arraySize--;
                    playMode.arraySize--;

                    reorderableList.index = dummyList.arraySize - 1;

                },
                onReorderCallbackWithDetails = (reorderableList, index, newIndex) =>
                {
                    var oldTitle = titles.GetArrayElementAtIndex(index).stringValue;
                    titles.GetArrayElementAtIndex(index).stringValue =
                        titles.GetArrayElementAtIndex(newIndex).stringValue;
                    titles.GetArrayElementAtIndex(newIndex).stringValue = oldTitle;

                    var orlURLString = urlString.GetArrayElementAtIndex(index).stringValue;
                    urlString.GetArrayElementAtIndex(index).stringValue =
                        urlString.GetArrayElementAtIndex(newIndex).stringValue;
                    urlString.GetArrayElementAtIndex(newIndex).stringValue = orlURLString;

                    var oldDescription = descriptions.GetArrayElementAtIndex(index).stringValue;
                    descriptions.GetArrayElementAtIndex(index).stringValue =
                        descriptions.GetArrayElementAtIndex(newIndex).stringValue;
                    descriptions.GetArrayElementAtIndex(newIndex).stringValue = oldDescription;

                    var oldPlayMode = playMode.GetArrayElementAtIndex(index).intValue;
                    playMode.GetArrayElementAtIndex(index).intValue =
                        playMode.GetArrayElementAtIndex(newIndex).intValue;
                    playMode.GetArrayElementAtIndex(newIndex).intValue = oldPlayMode;
                },
                drawElementCallback = (rect, index, active, focused) =>
                {
                    Rect uiRect = new Rect(rect.x, rect.y + 5, rect.width, EditorGUIUtility.singleLineHeight);

                    titles.GetArrayElementAtIndex(index).stringValue = EditorGUI.TextField(uiRect,
                        $"Video Title No. {index + 1}", titles.GetArrayElementAtIndex(index).stringValue);
                    uiRect.y += uiRect.height + 5;
                    
                    urlString.GetArrayElementAtIndex(index).stringValue = EditorGUI.TextField(uiRect,
                        $"URL", urlString.GetArrayElementAtIndex(index).stringValue);

                    uiRect.y += uiRect.height + 5;
                    descriptions.GetArrayElementAtIndex(index).stringValue = EditorGUI.TextField(uiRect, $"Description",
                        descriptions.GetArrayElementAtIndex(index).stringValue);

                    uiRect.y += uiRect.height + 5;
                    playMode.GetArrayElementAtIndex(index).intValue = EditorGUI.IntField(uiRect,
                        "PlayMode", playMode.GetArrayElementAtIndex(index).intValue);

                },
                elementHeightCallback = reorderableList => (EditorGUIUtility.singleLineHeight + 3) * 5,
                drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Video Playlist (PlayMode 0=Video, 1=Stream(Live) )"),

            };

        }

        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawConvertToUdonBehaviourButton(target) ||
                UdonSharpGUI.DrawProgramSource(target))
                return;

            serializedObject.Update();

            EditorGUILayout.PropertyField(kinelVideoPlayer);
            EditorGUILayout.Space();
            
            EditorGUILayout.PropertyField(autoPlay);
            EditorGUILayout.Space();
            list.DoLayoutList();
            EditorGUILayout.Space();
            
            showReference = EditorGUILayout.Foldout(showReference, "Object References");

            if (showReference)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(content);
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(videoPrefab);
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(warningUI);
                EditorGUILayout.Space();
                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
            
            var url = new List<VRCUrl>();
            for (int i = 0; i < urlString.arraySize; i++)
                url.Add(new VRCUrl(urlString.GetArrayElementAtIndex(i).stringValue));

            kinelPlaylist.urls = url.ToArray();
        }

    }
#endif
}
