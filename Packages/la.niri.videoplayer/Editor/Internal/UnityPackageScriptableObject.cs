using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Kinel.VideoPlayer.Editor.Internal
{
    [CreateAssetMenu(fileName = "UnityPackageScriptableObject", menuName = "ScriptableObject/UnityPackage")]
    public class UnityPackageScriptableObject : ScriptableObject
    {
        [SerializeField] Object[] objects;

        public Object[] Files
        {
            get { return objects; }
        }
    }

    [CustomEditor(typeof(UnityPackageScriptableObject))]
    public class UnityPackageScriptableObjectEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var targetPackage = (UnityPackageScriptableObject) target;
            base.OnInspectorGUI();

            if (!GUILayout.Button("Export unityPackage"))
            {
                return;
            }

            const string ext = "unitypackage";
            var path = EditorUtility.SaveFilePanel("Export unitypackage",
                Path.GetDirectoryName(AssetDatabase.GetAssetPath(targetPackage)),
                targetPackage.name + "." + ext, ext);

            if (!string.IsNullOrEmpty(path))
            {
                Export(targetPackage, path);
            }
        }

        public static void Export(UnityPackageScriptableObject package, string path)
        {
            EditorUtility.DisplayProgressBar("Exportiong package", "Exporting package", 0.5f);

            AssetDatabase.ExportPackage(
                package.Files.Select(AssetDatabase.GetAssetPath).ToArray(),
                path, ExportPackageOptions.Interactive | ExportPackageOptions.Recurse);
            EditorUtility.ClearProgressBar();
        }
    }
}