using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.UIElements;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;

namespace Kinel.VideoPlayer.Scripts.Playlist
{
    public class KinelPlaylistEditor : EditorWindow
    {
        
        [SerializeField] private List<string> tags = new List<string>();

        private KinelPlaylistEditor instance;
        private GameObject targetGameObject;
        private ReorderableList list;

        private ScriptableObject kinelPlaylistEditor;
        private SerializedObject serializedObject;
        private SerializedProperty tagsScriptableObject;

        [MenuItem("Editor/KinelPlaylistEditor")]
        private static void Create()
        {
            var window = GetWindow<KinelPlaylistEditor>("Kinel Playlist Editor");
            window.minSize = new Vector2(300, 300);
        }

        private void OnEnable()
        {
            instance = this;
            serializedObject = new SerializedObject(instance);
            tagsScriptableObject = serializedObject.FindProperty("tags");
            list = new ReorderableList(serializedObject, tagsScriptableObject);

            list.drawElementCallback = (rect, index, active, focused) =>
            {
                Rect uiRect = new Rect(rect.x, rect.y + 5, rect.width, EditorGUIUtility.singleLineHeight);

                tagsScriptableObject.GetArrayElementAtIndex(index).stringValue = EditorGUI.TextField(uiRect,
                    $"Tag name {index + 1}", tagsScriptableObject.GetArrayElementAtIndex(index).stringValue);
            };
            list.elementHeightCallback = reorderableList => (EditorGUIUtility.singleLineHeight + 10);
            list.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Playlist tab initializer");

        }

        private void OnGUI()
        {
            serializedObject.Update();
            
            EditorGUILayout.Space();
            targetGameObject = EditorGUILayout.ObjectField("設定先GameObject", targetGameObject, typeof(GameObject), true) as GameObject;
            EditorGUILayout.Space();
            list.DoLayoutList();
            EditorGUILayout.Space();
            serializedObject.ApplyModifiedProperties();
            if (GUILayout.Button("Apply"))
                CreateTabPrefab();

            if (GUILayout.Button("Reset Position"))
                ResetPosition();
            
            if (GUILayout.Button("Delete"))
                Delete();

        }

        private bool CreateTabPrefab()
        {
            if (!targetGameObject)
            {
                ShowNotification(new GUIContent("設定先のGameObjectを選択してください。"));
                return false;
            }

            if (tags.Count < 0){
                ShowNotification(new GUIContent("タグを一つ以上設定してください。"));
                return false;
            }

            var tabParent = targetGameObject.transform.Find("Canvas/Background_bottom/Tag/Viewport/Content").gameObject;
            var tabPrefab = tabParent.transform.Find("TabPrefab").gameObject;
            var listParent = targetGameObject.transform.Find("Canvas/Background_bottom/List").gameObject;
            var listPrefab = listParent.transform.Find("ListPrefab").gameObject;
            
            if(tabParent.transform.childCount > 1)
                this.Delete();

            for (int i = 0; i < tags.Count; i++)
            {
                var tab = Instantiate(tabPrefab);
                var playlist = Instantiate(listPrefab);
                var tagName = tagsScriptableObject.GetArrayElementAtIndex(i).stringValue;
                
                tab.transform.SetParent(tabParent.transform);
                playlist.transform.SetParent(listParent.transform);
                tab.transform.localPosition = Vector3.zero;
                tab.transform.localRotation = Quaternion.identity;
                tab.transform.localScale    = Vector3.one;
                playlist.transform.localPosition = new Vector3(-325, -450, 0);
                playlist.transform.localRotation = Quaternion.identity;
                playlist.transform.localScale = Vector3.one;

                var toggleComponet = tab.GetComponent<Toggle>();
                toggleComponet.isOn = (i == 0 ? true : false);
               
                
                tab.name = $"Tag {i + 1}:{tagName}";

                var text = tab.transform.GetChild(1).GetComponent<Text>();
                text.text = $"{tagName}";
                playlist.name = $"List {i + 1}:{tagName}";
                if(i >= 1)
                    playlist.gameObject.SetActive(false);
            }
            tabPrefab.gameObject.SetActive(false);
            listPrefab.gameObject.SetActive(false);
            return true;
        }

        private void ResetPosition()
        {
            var listParent = targetGameObject.transform.Find("Canvas/Background_bottom/List");
            for (int i = 0; i < listParent.childCount; i++)
            {
                var transform = listParent.GetChild(i);
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;

            }
        }


        private void Delete()
        {
            var tagParent = targetGameObject.transform.Find("Canvas/Background_bottom/Scroll View/Viewport/Content");
            var listParebt = targetGameObject.transform.Find("Canvas/Background_bottom/List");
            var count = tagParent.childCount;
            for (int i = 1; i < count; i++)
            {
                DestroyImmediate(tagParent.GetChild(1).gameObject);
                DestroyImmediate(listParebt.GetChild(1).gameObject);
            }
            tagParent.GetChild(0).gameObject.SetActive(true);
            listParebt.GetChild(0).gameObject.SetActive(true);
        }

        public List<string> Tags
        {
            get => tags;
            set => tags = value;
        }
    }
}