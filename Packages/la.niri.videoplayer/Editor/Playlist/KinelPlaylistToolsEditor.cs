using Kinel.VideoPlayer.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kinel.VideoPlayer.Scripts.v2.Editor
{
    public class KinelPlaylistToolsEditor : EditorWindow
    {
        // [MenuItem("Kienl/VideoPlayer/PlaylistToolsEditor")]
        // public static void ShowWindow() => GetWindow<KinelPlaylistEditor>("PlaylistTools");

        private static SerializedObject _serializedObject;

        private KinelPlaylistEditor editor ;

        [SerializeField] private bool loop, autoPlay;

        private void OnEnable()
        {
            // editor = KinelPlaylistEditor.GetInstance();
            _serializedObject = new SerializedObject(this);
        
            var root = rootVisualElement;
            root.Bind(_serializedObject);
            // 先ほど作ったUXMLファイルを読み込む
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Kinel/VideoPlayer/UI/PlaylistToolsEditor.uxml");
            // UXMLファイルで定義した階層構造を適用する
            visualTree.CloneTree(root);
        }
    
    
    
    }
}