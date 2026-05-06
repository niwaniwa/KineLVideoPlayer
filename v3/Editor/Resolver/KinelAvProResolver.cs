using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using UnityEditor;
using UnityEngine;

namespace Kinel.VideoPlayer.V3.Editor
{
    public class KinelAvProResolver : BaseKinelVideoPlayerEditor
    {
        public static readonly string AvProPath = Path.Combine(Application.dataPath, "AVProVideo");
        private static readonly string DownloadURL = "https://github.com/RenderHeads/UnityPlugin-AVProVideo/releases/download/3.2.0/UnityPlugin-AVProVideo-v3.2.0-Trial.unitypackage";

        [MenuItem(MenuItemParentPath + ToolsPath + "AvPro Download")]
        public static async void DownloadAvProVideoAssets()
        {
            if (!Directory.Exists(AvProPath))
            {
                try
                {
                    var savePath = Path.Combine(Application.dataPath, "avpro.unitypackage");
            
                    using (HttpClient client = new HttpClient())
                    {
                        try
                        {
                            using (HttpResponseMessage response = await client.GetAsync(DownloadURL, HttpCompletionOption.ResponseHeadersRead))
                            {
                                response.EnsureSuccessStatusCode();

                                using (Stream contentStream = await response.Content.ReadAsStreamAsync(),
                                       fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                                {
                                    await contentStream.CopyToAsync(fileStream);
                                }
                            }

                            Debug.Log($"{DebugLogPrefix} {savePath} に保存しました。");
                        }
                        catch (Exception ex)
                        {
                            Debug.Log($"{DebugLogPrefix} ダウンロード中にエラーが発生しました: {ex.Message}");
                            return;
                        }
                    }
            
            
                    AssetDatabase.ImportPackage(Path.Combine(Application.dataPath, "avpro.unitypackage"), true);
                }
                catch (Exception e)
                {
                    throw; // TODO handle exception
                }
            }

            
        }

# if !KINEL_AVPRO_VIDEO_ENABLED
        [MenuItem(MenuItemParentPath + ToolsPath + "Setup AvProVideo")]
        public static void SetupAvProVideoResolver()
        {
            if (!Directory.Exists(AvProPath))
            {
                EditorUtility.DisplayDialog("お知らせ", $"AvPro Downloadからダウンロードしてください。", "ok");
                return;
            }
            //
            // var settings = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone).Split(";").Concat(new[] { "KINEL_AVPRO_VIDEO_ENABLED" }).ToArray();
            //
            // PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, settings);
            // PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, settings);
            // PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, settings);
            //
            var symbol = "KINEL_AVPRO_VIDEO_ENABLED";
            
            KinelEditorUtility.AddScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, symbol);
            KinelEditorUtility.AddScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, symbol);
            KinelEditorUtility.AddScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, symbol);
            
        }
#else
        [MenuItem(MenuItemParentPath + ToolsPath + "Disable AvProVideo")]
        public static void SetupAvProVideoResolver()
        {
            var settings = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone).Split(";")
                .ToList();
            settings.Remove("KINEL_AVPRO_VIDEO_ENABLED");
            var reConstructedSettings = settings.ToArray();

            

            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, reConstructedSettings);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, reConstructedSettings);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, reConstructedSettings);
            
            // var symbol = "KINEL_AVPRO_VIDEO_ENABLED";
            //
            // KinelEditorUtility.RemoveScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, symbol);
            // KinelEditorUtility.RemoveScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, symbol);
            // KinelEditorUtility.RemoveScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, symbol);
        }

#endif
    }
}