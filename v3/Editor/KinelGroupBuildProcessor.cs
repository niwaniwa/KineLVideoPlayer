using System.Collections.Generic;
using System.Linq;
using Kinel.VideoPlayer.V3.Scripts;
using Kinel.VideoPlayer.V3.Udon.Interface;
using Kinel.VideoPlayer.V3.Udon.System;
using Kinel.VideoPlayer.V3.Udon.System.Component;
using Kinel.VideoPlayer.V3.Udon.System.Sync;
using UdonSharp;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;
using Kinel.VideoPlayer.V3.Udon.Yttl;

namespace Kinel.VideoPlayer.V3.Editor
{
    public class KinelGroupBuildProcessor : IProcessSceneWithReport
    {
        private const string LogPrefix = "[<color=#f0e68c>KineL</color>][Group]";

        public int callbackOrder => -100;

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            var tags = scene.GetRootGameObjects()
                .SelectMany(go => go.GetComponentsInChildren<KinelGroupTag>(true))
                .Where(t => t.Group != null)
                .ToList();

            if (tags.Count == 0)
                return;

            var groups = new Dictionary<KinelGroupConfig, List<KinelGroupTag>>();
            foreach (var tag in tags)
            {
                if (!groups.TryGetValue(tag.Group, out var list))
                {
                    list = new List<KinelGroupTag>();
                    groups[tag.Group] = list;
                }

                list.Add(tag);
            }

            foreach (var kv in groups)
            {
                var config = kv.Key;
                var groupTags = kv.Value;
                var groupName = string.IsNullOrEmpty(config.DisplayName) ? config.name : config.DisplayName;

                var coresInGroup = groupTags
                    .Select(t => t.GetComponent<KinelPlayerController>())
                    .Where(c => c != null)
                    .Distinct()
                    .ToList();

                if (coresInGroup.Count == 0)
                {
                    throw new BuildFailedException(
                        $"{LogPrefix} Group '{groupName}' has no KinelPlayerController. " +
                        "Attach a KinelGroupTag to the Core GameObject.");
                }

                if (coresInGroup.Count > 1)
                {
                    var coreNames = string.Join(", ", coresInGroup.Select(c => c.name));
                    throw new BuildFailedException(
                        $"{LogPrefix} Group '{groupName}' has {coresInGroup.Count} KinelPlayerController instances ({coreNames}). " +
                        "Each group must contain exactly one Core.");
                }

                var core = coresInGroup[0];
                var wired = 0;

                foreach (var tag in groupTags)
                {
                    var go = tag.gameObject;
                    wired += WireField<KinelUIController>(go, "controller", core);
                    wired += WireField<KinelVariableSyncer>(go, "controller", core);
                    wired += WireField<KinelABLoop>(go, "controller", core);
                    wired += WireField<KinelABLoopUI>(go, "controller", core);
                    wired += WireField<KinelPlaybackHistory>(go, "videoPlayer", core);
                    wired += WireField<KinelYttlBridge>(go, "controller", core);
                }

                Debug.Log($"{LogPrefix} Group '{groupName}': wired {wired} field(s) to Core '{core.name}'.");
            }

            foreach (var tag in tags)
            {
                Object.DestroyImmediate(tag);
            }
        }

        private static int WireField<T>(GameObject go, string fieldName, KinelPlayerController core)
            where T : UdonSharpBehaviour
        {
            var components = go.GetComponents<T>();
            var count = 0;
            foreach (var component in components)
            {
                if (component == null) continue;

                var so = new SerializedObject(component);
                var prop = so.FindProperty(fieldName);
                if (prop == null)
                {
                    Debug.LogWarning(
                        $"{LogPrefix} Field '{fieldName}' not found on {typeof(T).Name} ({go.name}). Skipped.");
                    continue;
                }

                prop.objectReferenceValue = core;
                so.ApplyModifiedPropertiesWithoutUndo();
                component.SetProgramVariable(fieldName, core);
                count++;
            }

            return count;
        }
    }
}