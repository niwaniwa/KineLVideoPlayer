using Kinel.VideoPlayer.V3.Scripts;
#if KINEL_AVPRO_VIDEO_ENABLED    
using RenderHeads.Media.AVProVideo;
#endif
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine.SceneManagement;
using VRC.SDK3.Video.Components.AVPro;

namespace Kinel.VideoPlayer.V3.Editor
{
    public class KinelBuildProcessor : IProcessSceneWithReport
    {
        public int callbackOrder { get => 0; }
        public void OnProcessScene(Scene scene, BuildReport report)
        {
#if KINEL_AVPRO_VIDEO_ENABLED
            VRCAVProVideoPlayer.Initialize += player =>
            {
                var avpro = player.gameObject.AddComponent<MediaPlayer>();
                avpro.AutoOpen = false;
                avpro.AutoStart = false;
                return new KinelAvProVideoResolver(player, avpro);
            };

            VRCAVProVideoScreen.Initialize += screen =>
            {
                var avProScreen = screen.gameObject.AddComponent<KinelAvProScreen>();
                avProScreen.VideoScreen = screen;
            };
#endif
        }
    }
}