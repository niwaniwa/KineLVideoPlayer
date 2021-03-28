#if UNITY_EDITOR && !COMPILER_UDONSHARP
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.UIElements;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;

namespace Kinel.VideoPlayer.Scripts.Playlist
{

    public class KinelPlayListGenerator : MonoBehaviour
    {
        public GameObject playlist;
        
        public List<string> tags = new List<string>();
        public List<KinelPlaylist> playlistList = new List<KinelPlaylist>();
        public int index = 0;
        public string[] playerList = { "KinelVideoPlayer", "iwasync v3", "USharpVideo"};
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

//      private List<KinelPlaylist> playlistList = new List<KinelPlaylist>();

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
                CreateTabPrefab();

//            if (GUILayout.Button("Reset Position")) 
//                ResetPosition();

            if (GUILayout.Button("Delete"))
            {
                bool isOK = EditorUtility.DisplayDialog("警告", "すべてのデータが消去されます。続行しますか?", "続行", "中止");
                if (!isOK)
                    return;
                
                Delete();
            }
            
            
//            if (GUILayout.Button("Load"))  // to do
//                Delete();
            
//            if (GUILayout.Button("Export"))  // to do
//                Export();

        }

        private bool CreateTabPrefab()
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
                var tab = Instantiate(tabPrefab);
                var playlist = Instantiate(listPrefab);
                var tagName = tagsSerializedProperty.GetArrayElementAtIndex(i).stringValue;
                
                tab.transform.SetParent(tabParent.transform);
                playlist.transform.SetParent(listParent.transform);
                tab.transform.localPosition = Vector3.zero;
                tab.transform.localRotation = Quaternion.identity;
                tab.transform.localScale    = Vector3.one;
                playlist.transform.localPosition = new Vector3(-325, -450, 0);
                playlist.transform.localRotation = Quaternion.identity;
                playlist.transform.localScale = Vector3.one;

                var toggleComponet = tab.GetComponent<Toggle>();
                toggleComponet.isOn = (i == indexSerializedProperty.intValue - 1 ? true : false);

                var playlistComponent = playlist.GetComponent<KinelPlaylist>();

                tab.name = $"Tag {i + 1}:{tagName}";

                var text = tab.transform.GetChild(1).GetComponent<Text>();
                text.text = $"{tagName}";
                playlist.name = $"List {i + 1}:{tagName}";
                if(i != indexSerializedProperty.intValue - 1)
                    playlist.gameObject.SetActive(false);

//                instance.playlistList.Add(playlistComponent);
            }
            tabPrefab.gameObject.SetActive(false);
            listPrefab.gameObject.SetActive(false);
            return true;
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

        public void Export()
        {
            // to do
        }

    }
}
#endif