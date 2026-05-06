using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Kinel.VideoPlayer.V3.Editor
{
    public class KinelEditorUtility
    {
        public static void AddScriptingDefineSymbolsForGroup(BuildTargetGroup group, params string[] symbols)
        {
            var settings = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone).Split(";")
                .Concat(symbols).ToArray();
            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, settings);
        }

        public static void RemoveScriptingDefineSymbolsForGroup(BuildTargetGroup group, params string[] symbols)
        {
            var settings = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone).Split(";")
                .ToList();
            foreach (var symbol in symbols)
            {
                settings.Remove(symbol);
            }

            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, string.Join(";", settings));
        }

        public static void AddAssemblyDefinitionReference(string asmdefPath, string guid)
        {
            if (File.Exists(asmdefPath))
            {
                var asmdefContent = File.ReadAllText(asmdefPath);
                var asmdefData = JsonUtility.FromJson<AssemblyDefinitionData>(asmdefContent);

                if (!asmdefData.references.Contains(guid))
                {
                    asmdefData.references = asmdefData.references.Concat(new[] { guid }).ToArray();
                    var updatedContent = JsonUtility.ToJson(asmdefData);
                    File.WriteAllText(asmdefPath, updatedContent);
                    AssetDatabase.Refresh();
                }
            }
        }

        public static void RemoveAssemblyDefinitionReference(string asmdefPath, string guid)
        {
            if (File.Exists(asmdefPath))
            {
                var asmdefContent = File.ReadAllText(asmdefPath);
                var asmdefData = JsonUtility.FromJson<AssemblyDefinitionData>(asmdefContent);

                if (asmdefData.references.Contains(guid))
                {
                    asmdefData.references = asmdefData.references.Where(r => r != guid).ToArray();
                    var updatedContent = JsonUtility.ToJson(asmdefData, true);
                    File.WriteAllText(asmdefPath, updatedContent);
                    AssetDatabase.Refresh();
                }
            }
        }

        /// <summary>
        /// GUID からプレハブをロード（Editor 限定）
        /// </summary>
        public static T LoadPrefabByGUID<T>(string guid) where T : Object
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(assetPath))
            {
                Debug.LogError($"Invalid GUID: {guid}");
                return null;
            }

            return AssetDatabase.LoadAssetAtPath<T>(assetPath);
        }

        [System.Serializable]
        private class AssemblyDefinitionData
        {
            public string name;
            public string rootNamespace;
            public string[] references;
            public string[] includePlatforms;
            public string[] excludePlatforms;
            public bool allowUnsafeCode;
            public bool overrideReferences;
            public string[] precompiledReferences;
            public bool autoReferenced;
            public string[] defineConstraints;
            public string[] versionDefines;
            public bool noEngineReferences;
        }
    }
}