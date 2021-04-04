#if UNITY_EDITOR && !COMPILER_UDONSHARP
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UdonSharp;
using UdonSharpEditor;
using UnityEditor;
using UnityEditor.Experimental.UIElements;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;
using VRC.Udon;
using VRC.Udon.Serialization.OdinSerializer.Utilities;

namespace Kinel.VideoPlayer.Scripts.Playlist
{

    public class KinelPlayListGenerator : MonoBehaviour
    {
        public GameObject playlist;
        
        public List<string> tags = new List<string>();
        public List<KinelPlaylist> playlistList = new List<KinelPlaylist>();
        public int index = 0;
        public string[] playerList = { "KinelVideoPlayer", "iwasync v3", "USharpVideo"};

        public void OnEnable()
        {
            
        }
    }
    
    [CustomEditor(typeof(KinelPlayListGenerator))]
    public class KinelPlayListGeneratorInspector : Editor
    {

        private KinelPlayListGenerator instance;
        private GameObject targetGameObject;
        private SerializedProperty playlistSerializedProperty;
        private ReorderableList list;

        private ScriptableObject kinelPlaylistEditor;
        private SerializedProperty tagsSerializedProperty;
        private SerializedProperty indexSerializedProperty;

        private void OnEnable()
        {
            instance = target as KinelPlayListGenerator;
            playlistSerializedProperty = serializedObject.FindProperty(nameof(KinelPlayListGenerator.playlist));
            tagsSerializedProperty = serializedObject.FindProperty(nameof(KinelPlayListGenerator.tags));
            indexSerializedProperty = serializedObject.FindProperty(nameof(KinelPlayListGenerator.index));

            list = new ReorderableList(serializedObject, tagsSerializedProperty);

            list.drawElementCallback = (rect, index, active, focused) =>
            {
                Rect uiRect = new Rect(rect.x, rect.y + 5, rect.width, EditorGUIUtility.singleLineHeight);

                tagsSerializedProperty.GetArrayElementAtIndex(index).stringValue = EditorGUI.TextField(uiRect,
                    $"Tag name {index + 1}", tagsSerializedProperty.GetArrayElementAtIndex(index).stringValue);
            };
            list.elementHeightCallback = reorderableList => (EditorGUIUtility.singleLineHeight + 10);
            list.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Playlist tab initializer");

        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            
            serializedObject.Update();
            
            EditorGUILayout.Space();
//          instance.playlist = EditorGUILayout.ObjectField("Playlist_tab Object", instance.playlist, typeof(GameObject), true) as GameObject;
            EditorGUILayout.PropertyField(playlistSerializedProperty);
            EditorGUILayout.Space();
//          var index = EditorGUILayout.Popup("using player", instance.index, instance.playerList);
            EditorGUILayout.PropertyField(indexSerializedProperty);
            EditorGUILayout.Space();
            list.DoLayoutList();
            EditorGUILayout.Space();
            serializedObject.ApplyModifiedProperties();
            
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(instance, "playlist");
            }
            
            if (GUILayout.Button("Generate"))
                CreateTabPrefabs();

//            if (GUILayout.Button("Reset Position")) 
//                ResetPosition();

            if (GUILayout.Button("Delete"))
            {
                bool isOK = EditorUtility.DisplayDialog("警告", "すべてのデータが消去されます。続行しますか?", "続行", "中止");
                if (!isOK)
                    return;
                
                Delete();
            }


            if (GUILayout.Button("Import"))
            {
                if(instance.playlistList.Count > 0)
                {
                    var overwrite = EditorUtility.DisplayDialogComplex("上書き", "既にListが存在します。", "続けて追加", "中止", "削除して追加");
                    
                    switch (overwrite)
                    {
                        case 0:
                            ImportAll(true);
                            break;
                        case 2:
                            ImportAll(false);
                            break;
                        default:
                            return;
                    }

                    return;
                }
                ImportAll(false);

            }
            
            if (GUILayout.Button("Export"))  // to do
                ExportAll();

        }

        private bool CreateTabPrefabs()
        {
            if (!instance.playlist)
            {
                Debug.LogError("instance.playlist is null");
                return false;
            }

            if (instance.tags.Count < 0){
                Debug.LogError("tag count is 0");
                return false;
            }

            var tabParent = instance.playlist.transform.Find("Canvas/Playlist/Tag/Viewport/Content").gameObject;
            var tabPrefab = tabParent.transform.Find("TabPrefab").gameObject;
            var listParent = instance.playlist.transform.Find("Canvas/Playlist/List").gameObject;
            var listPrefab = listParent.transform.Find("ListPrefab").gameObject;
            
            if(tabParent.transform.childCount > 1)
            {
                bool isOK = EditorUtility.DisplayDialog("警告", "既にListが存在します。続行すると上書きされます。", "続行", "中止");
                if (!isOK)
                    return false;
                
                this.Delete();
            }

            for (int i = 0; i < instance.tags.Count; i++)
            {
                CreateTabPrefab(tagsSerializedProperty.GetArrayElementAtIndex(i).stringValue, i,
                    tabPrefab, listPrefab, tabParent, listParent, (i == indexSerializedProperty.intValue - 1 ? true : false));
            }
            tabPrefab.gameObject.SetActive(false);
            listPrefab.gameObject.SetActive(false);
            return true;
        }

        private GameObject CreateTabPrefab(string tagName, int index, GameObject tabPrefab, GameObject listPrefab, GameObject tabParent, GameObject listParent, bool isActive)
        {
            var tab = Instantiate(tabPrefab);
            var playlist = Instantiate(listPrefab);

            tab.transform.SetParent(tabParent.transform);
            playlist.transform.SetParent(listParent.transform);
            tab.transform.localPosition = Vector3.zero;
            tab.transform.localRotation = Quaternion.identity;
            tab.transform.localScale    = Vector3.one;
            tab.name = $"Tag {index + 1} {tagName}";
            tab.SetActive(true);
            
            playlist.transform.localPosition = new Vector3(-325, -450, 0);
            playlist.transform.localRotation = Quaternion.identity;
            playlist.transform.localScale = Vector3.one;

            var toggleComponet = tab.GetComponent<Toggle>();
            toggleComponet.isOn = isActive;

            var playlistComponent = playlist.GetComponent<KinelPlaylist>();

            var text = tab.transform.GetChild(1).GetComponent<Text>();
            text.text = $"{tagName}";
            playlist.name = $"List {index + 1} {tagName}";

            instance.playlistList.Add(playlistComponent);
            playlist.SetActive(isActive);
            return playlist;
        }

        private void ResetPosition()
        {
            var listParent = instance.playlist.transform.Find("Canvas/Playlist/List");
            for (int i = 0; i < listParent.childCount; i++)
            {
                var transform = listParent.GetChild(i);
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;

            }
        }

        private void Delete()
        {
            var tagParent = instance.playlist.transform.Find("Canvas/Playlist/Tag/Viewport/Content");
            var listParebt = instance.playlist.transform.Find("Canvas/Playlist/List");
            var count = tagParent.childCount;
            for (int i = 1; i < count; i++)
            {
                DestroyImmediate(tagParent.GetChild(1).gameObject);
                DestroyImmediate(listParebt.GetChild(1).gameObject);
            }
            tagParent.GetChild(0).gameObject.SetActive(true);
            listParebt.GetChild(0).gameObject.SetActive(true);
            instance.playlistList.RemoveAll(s => true);
        }

        public void ExportAll()
        {
            var filePath = EditorUtility.SaveFolderPanel("", "", "Playlist");

            if (string.IsNullOrEmpty(filePath))
                return;

            filePath = $"{filePath}/{DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss")}/";
            
            Directory.CreateDirectory(filePath);

            for (int i = 0; i < instance.playlistList.Count; i++)
            {
                var playList = instance.playlistList[i];
                var jsonString = JsonUtility.ToJson(playList);
                File.WriteAllText($"{filePath}/{playList.name}.json", jsonString);
            }
        }

        public void ExportPlaylist(string name)
        {
            
        }

        public void ImportAll(bool Overwrite)
        {

            var directoryPath = EditorUtility.SaveFolderPanel("Import", "", "Playlist");

            if (String.IsNullOrEmpty(directoryPath))
                return;
            
            if (!Overwrite)
            {
                Delete();
            }
            
            var tabParent = instance.playlist.transform.Find("Canvas/Playlist/Tag/Viewport/Content").gameObject;
            var tabPrefab = tabParent.transform.Find("TabPrefab").gameObject;
            var listParent = instance.playlist.transform.Find("Canvas/Playlist/List").gameObject;
            var listPrefab = listParent.transform.Find("ListPrefab").gameObject;
            
            foreach (var path in Directory.GetFiles(directoryPath))
            {
                if (!path.Contains("json"))
                {
                    Debug.LogError($"[KineL] Import error! please check your file extention {path}");
                    return;
                }
                
                if(path.Contains("meta"))
                    continue;
                
                var jsonString = File.ReadAllText(path);
                var splitedPath = path.Split(Path.DirectorySeparatorChar);
                var str = splitedPath[splitedPath.Length - 1].Split(' ');
                var tag = "";
                for (int i = 2; i < str.Length; i++)
                {
                    tag = tag + " " + str[i];
                }
                tag = tag.Replace(".json", "");

                bool active = false;
                
                if(!Overwrite)
                    if (instance.playlistList.Count - 1 == indexSerializedProperty.intValue - 2)
                        active = true;

                var playlistObject = CreateTabPrefab(tag, instance.playlistList.Count, tabPrefab, listPrefab, tabParent, listParent, active);
                var playlist = playlistObject.GetComponents<KinelPlaylist>();
                JsonUtility.FromJsonOverwrite(jsonString, playlist[0]);
                
                UdonSharpEditorUtility.ConvertToUdonBehaviours(playlist, true);
                DestroyImmediate(playlistObject.GetComponents<UdonBehaviour>()[0]);

            }
            
            if (!Overwrite)
            {
                tabPrefab.SetActive(false);
                listPrefab.SetActive(false);
            }
            
            
        }

        public void Import(string fileName)
        {
            
        }

        private void ConvertUdonSharpScript(GameObject obj)
        {
            var playlist = obj.GetComponents<KinelPlaylist>();

            UdonSharpEditorUtility.ConvertToUdonBehaviours(playlist, true);
            
            Debug.Log($"{playlist.Length}");
            
            var playlistSerializedObject = new SerializedObject(playlist[0]);
            // playlist[0].content =
            playlistSerializedObject.FindProperty(nameof(KinelPlaylist.content)).objectReferenceValue 
                = obj.transform.Find("Viewport/Content").transform as RectTransform;
            playlistSerializedObject.FindProperty(nameof(KinelPlaylist.videoPrefab)).objectReferenceValue 
                = obj.transform.Find("Viewport/VideoPrefab").gameObject;
            playlistSerializedObject.FindProperty(nameof(KinelPlaylist.warningUI)).objectReferenceValue 
                = instance.gameObject.transform.Find("Canvas/WarningImage").gameObject;
            
            playlistSerializedObject.Update();
            
            playlistSerializedObject.FindProperty(nameof(KinelPlaylist.videoPrefab));
            playlistSerializedObject.FindProperty(nameof(KinelPlaylist.warningUI));

            //        ugokanai
            //        var oldUdon = obj.GetComponents<KinelPlaylist>();
            //        foreach (var udonSharp in oldUdon)
            //        {
            //            DestroyImmediate(udonSharp);
            //        }



        }
        
        

    }
}
#endif