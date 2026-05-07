using System.Linq;
using UnityEditor;

namespace Kinel.VideoPlayer.V3.Editor
{
    public class KinelAvProDefineMenu : BaseKinelVideoPlayerEditor
    {
#if !KINEL_AVPRO_VIDEO_ENABLED
        [MenuItem(MenuItemParentPath + ToolsPath + "Setup AvProVideo")]
        public static void EnableAvProDefine()
        {
            var symbol = "KINEL_AVPRO_VIDEO_ENABLED";

            KinelEditorUtility.AddScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, symbol);
            KinelEditorUtility.AddScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, symbol);
            KinelEditorUtility.AddScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, symbol);
        }
#else
        [MenuItem(MenuItemParentPath + ToolsPath + "Disable AvProVideo")]
        public static void DisableAvProDefine()
        {
            var settings = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone).Split(";")
                .ToList();
            settings.Remove("KINEL_AVPRO_VIDEO_ENABLED");
            var reConstructedSettings = settings.ToArray();

            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, reConstructedSettings);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, reConstructedSettings);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, reConstructedSettings);
        }
#endif
    }
}
