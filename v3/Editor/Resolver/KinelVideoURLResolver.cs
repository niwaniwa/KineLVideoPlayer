using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Kinel.VideoPlayer.V3.Scripts;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Components.Video;
using VRC.SDK3.Video.Components;
using VRC.SDKBase;

namespace Kinel.VideoPlayer.V3.Editor
{
    [InitializeOnLoad]
    public class KinelVideoURLResolver : BaseKinelVideoPlayerEditor
    {
        public static string YtdlpPath;
        public static string UtilitiesPath = "Kinel";

        private static readonly string _applicationYtdlpPath;

        private static readonly string _vrChatYtdlpPath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData", "LocalLow",
                "VRChat", "VRChat", "Tools", "yt-dlp.exe");

        private static readonly string DownloadUrl;

        static KinelVideoURLResolver()
        {
            SetupYoutubeDL();
            VRCUnityVideoPlayer.StartResolveURLCoroutine = ResolveURL;
#if KINEL_AVPRO_VIDEO_ENABLED
            KinelAvProVideoResolver.StartResolveURLCoroutine = ResolveURL;
#endif

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)){
                DownloadUrl = "https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp.exe";
            }else if(RuntimeInformation.IsOSPlatform(OSPlatform.OSX)){
                DownloadUrl = "https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp";
            }else if(RuntimeInformation.IsOSPlatform(OSPlatform.Linux)){
                DownloadUrl = "https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp_linux";
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)){
                _applicationYtdlpPath = Path.Combine(Application.dataPath, UtilitiesPath, "yt-dlp.exe");
            }else if(RuntimeInformation.IsOSPlatform(OSPlatform.OSX)){
                _applicationYtdlpPath = Path.Combine(Application.dataPath, UtilitiesPath, "yt-dlp");
            }else if(RuntimeInformation.IsOSPlatform(OSPlatform.Linux)){
                _applicationYtdlpPath = Path.Combine(Application.dataPath, UtilitiesPath, "yt-dlp_linux");
            }
        }


        public static void ResolveURL(VRCUrl url, int maxResolution, UnityEngine.Object videoPlayer,
            Action<string> resolvedCallback, Action<VideoError> errorCallback)
        {
            if (String.IsNullOrEmpty(YtdlpPath))
            {
                errorCallback(VideoError.PlayerError);
            }

            Debug.Log($"{DebugLogPrefix} ResolveURL: {url}");

            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo ytdlpProcess = new System.Diagnostics.ProcessStartInfo();
            ytdlpProcess.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            ytdlpProcess.FileName = YtdlpPath;
            ytdlpProcess.Arguments =
                $"--no-check-certificates --no-cache-dir --rm-cache-dir -f \"bv[height<={maxResolution}][ext=mp4]+ba[height<=720][ext=m4a]/best[height<={maxResolution}][ext=mp4]\" -g {url}";
            ytdlpProcess.CreateNoWindow = true;
            ytdlpProcess.UseShellExecute = false;
            ytdlpProcess.RedirectStandardOutput = true;
            ytdlpProcess.RedirectStandardError = true;
            process.StartInfo = ytdlpProcess;
            process.Start();

            Debug.Log($"{DebugLogPrefix} ResolveURL: process.Start()");

            ResolveURLCallback(process, resolvedCallback, errorCallback,
                ((MonoBehaviour)videoPlayer).destroyCancellationToken).Forget();
        }

        private static async UniTask ResolveURLCallback(System.Diagnostics.Process process,
            Action<string> resolvedCallback,
            Action<VideoError> errorCallback, CancellationToken cancellationToken)
        {
            await UniTask.WaitUntil(() => process.HasExited, cancellationToken: cancellationToken);

            Debug.Log($"{DebugLogPrefix} ResolveURLCallback: process.HasExited: {process.HasExited}");

            if (cancellationToken.IsCancellationRequested)
            {
                Debug.Log($"{DebugLogPrefix} Cancellation requested.");
                errorCallback(VideoError.PlayerError);
                return;
            }

            var resolvedUrl = await process.StandardOutput.ReadToEndAsync();

            if (process.ExitCode != 0)
            {
                string errorOutput = await process.StandardError.ReadToEndAsync();
                Debug.LogError($"{DebugLogPrefix} Process exited with code {process.ExitCode}. Error: {errorOutput}");
                errorCallback(VideoError.PlayerError);
                return;
            }

            if (!string.IsNullOrWhiteSpace(resolvedUrl))
            {
                Debug.Log($"{DebugLogPrefix} ResolvedURL: {resolvedUrl}");
                resolvedCallback(resolvedUrl);
            }
            else
            {
                Debug.LogError($"{DebugLogPrefix} Resolved URL is empty.");
                errorCallback(VideoError.InvalidURL);
            }
        }

        [MenuItem(MenuItemParentPath + ToolsPath + "Setup YoutubeDL")]
        public static async UniTask ManualYoutubeDLSetup()
        {
            if (!EditorUtility.DisplayDialog("お知らせ", $"Youtube DLをダウンロードしますか?", "ok", "cancel"))
            {
                return;
            }

            var success = await DownloadYoutubeDLAsync(_applicationYtdlpPath);

            if (success)
                EditorUtility.DisplayDialog("お知らせ", $"yt-dlp を {_applicationYtdlpPath} に保存しました", "ok");
            else
                EditorUtility.DisplayDialog("お知らせ", $"ダウンロード中にエラーが発生しました", "ok");
        }

        public static void SetupYoutubeDL()
        {
            if (File.Exists(_applicationYtdlpPath))
            {
                YtdlpPath = _applicationYtdlpPath;
                return;
            }

            if (File.Exists(_vrChatYtdlpPath))
            {
                YtdlpPath = _vrChatYtdlpPath;
                return;
            }

            YtdlpPath = String.Empty;
        }

        public static async UniTask<bool> DownloadYoutubeDLAsync(string savePath)
        {
            if (File.Exists(savePath))
            {
                return true;
            }

            if (!Directory.Exists(Path.GetDirectoryName(savePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(savePath) ?? string.Empty);
            }

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    Debug.Log("yt-dlp をダウンロード中...");

                    using (HttpResponseMessage response =
                           await client.GetAsync(DownloadUrl, HttpCompletionOption.ResponseHeadersRead))
                    {
                        response.EnsureSuccessStatusCode();

                        using (Stream contentStream = await response.Content.ReadAsStreamAsync(),
                               fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.None,
                                   8192, true))
                        {
                            await contentStream.CopyToAsync(fileStream);
                        }
                    }

                    Debug.Log($"{DebugLogPrefix} yt-dlp を {savePath} に保存しました。");
                }
                catch (Exception ex)
                {
                    Debug.Log($"{DebugLogPrefix} ダウンロード中にエラーが発生しました: {ex.Message}");
                    return false;
                }
            }

            return true;
        }
    }
}
