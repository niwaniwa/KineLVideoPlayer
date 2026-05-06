using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Kinel.VideoPlayer.V3.Editor
{
    // [InitializeOnLoad]  // disabled in v3 skeleton phase
    public class KinelIwasyncDetector : BaseKinelVideoPlayerEditor
    {
        private const string IwasyncSymbol = "KINEL_IWASYNC";
        private const string IwasyncPackagePath = "Assets/HoshinoLabs/iwaSync3";
        private const string IwasyncAssemblyGuid = "GUID:2ea38240a9fc74b40b7bc09442591120";
        private const string KinelAsmdefPath = "Packages/KineLLocalVideoPlayer/Runtime/Udon/Kinel.VideoPlayer.V3.Udon.asmdef";
        
        static KinelIwasyncDetector()
        {
            EditorApplication.delayCall += CheckIwasyncPackage;
        }
        
        [MenuItem(MenuItemParentPath + ToolsPath + "Iwasync Detected")]
        public static void IwasyncDetected()
        {
            CheckIwasyncPackage();
        }
        
        private static void CheckIwasyncPackage()
        {
            bool iwasyncExists = Directory.Exists(IwasyncPackagePath);
            bool symbolExists = HasIwasyncSymbol();
            
            Debug.Log($"[Kinel] Iwasync exists: {iwasyncExists}, symbol exists: {symbolExists}");

            if (iwasyncExists && !symbolExists)
            {
                AddIwasyncSymbol();
                AddIwasyncAssemblyReference();
                Debug.Log($"[Kinel] Added Iwasync symbol and assembly reference");
            }
            else if (!iwasyncExists && symbolExists)
            {
                RemoveIwasyncSymbol();
                RemoveIwasyncAssemblyReference();
                Debug.Log($"[Kinel] Removed Iwasync symbol and assembly reference");
            }
        }
        
        private static bool HasIwasyncSymbol()
        {
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);
            return defines.Contains(IwasyncSymbol);
        }
        
        private static void AddIwasyncSymbol()
        {
            KinelEditorUtility.AddScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, IwasyncSymbol);
        }
        
        private static void RemoveIwasyncSymbol()
        {
            KinelEditorUtility.RemoveScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, IwasyncSymbol);
        }
        
        private static void AddIwasyncAssemblyReference()
        {
            KinelEditorUtility.AddAssemblyDefinitionReference(KinelAsmdefPath, IwasyncAssemblyGuid);
        }
        
        private static void RemoveIwasyncAssemblyReference()
        {
            KinelEditorUtility.RemoveAssemblyDefinitionReference(KinelAsmdefPath, IwasyncAssemblyGuid);
        }
        
    }
}