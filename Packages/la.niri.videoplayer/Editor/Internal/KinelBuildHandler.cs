

using System.Collections.Generic;
using System.Linq;
using Kinel.VideoPlayer.Scripts;
using Kinel.VideoPlayer.Udon;
using Kinel.VideoPlayer.Udon.Module;
using UdonSharp;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRC.Udon;

namespace Kinel.VideoPlayer.Editor.Internal
{
    public class KinelBuildHandler : IProcessSceneWithReport
    {
        
        protected internal const string DEBUG_LOG_PREFIX = "[<color=#008080>KineL</color>]";
        protected internal const string DEBUG_ERROR_PREFIX = "[<color=#dc143c>KineL</color>]";

        public int callbackOrder => 0;
        
        // Unity 2020 obsolete
        public void OnProcessScene(Scene scene, BuildReport report)
        {
            
            Debug.Log($"{DEBUG_LOG_PREFIX} Build process start...");
            Debug.Log($"{DEBUG_LOG_PREFIX} ::::: Check Scene {scene.name}");
            
            Debug.Log($"{DEBUG_LOG_PREFIX} ::::: Setup Kinel Editor ...");
            
            // 値の適用をしてほしいのは KinelUI, Kinel Playlist,
            var kinelUI = FindObjectsOfType<KinelUIScript>(true);
            var kinelPlaylist = FindObjectsOfType<KinelPlaylistScript>(true);
            var resolution = FindObjectsOfType<KinelResolutionChangerScript>(true);
            ApplyUdonProperties<KinelUIEditor>(kinelUI);
            ApplyUdonProperties<KinelPlaylistEditor>(kinelPlaylist);
            ApplyUdonProperties<KinelPlaylistEditor>(resolution);
            
            Debug.Log($"{DEBUG_LOG_PREFIX} ::::: Build process End");
        }

        private void ApplyUdonProperties<T>(Component[] components) where T : KinelEditorBase
        {
            foreach (var component in components)
            {
                var editor = (T) UnityEditor.Editor.CreateEditor(component, typeof(T));
                // Apply properties
                Debug.Log($"{DEBUG_LOG_PREFIX} {component.gameObject.transform.GetHierarchyPath()}");
                editor.ApplyUdonProperties();
                GameObject.DestroyImmediate(editor);
            }
        }

        private T[] FindObjectsOfType<T>(bool includeInactive) where T : Component
        {
            return Resources.FindObjectsOfTypeAll<T>();
        }
    }
}