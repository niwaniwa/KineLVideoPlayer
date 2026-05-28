using Kinel.VideoPlayer.V3.Udon.Interface;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using VRC.Udon;

namespace Kinel.VideoPlayer.V3.Editor
{
    public class KinelButtonHoverRelayBuildProcessor : BaseKinelVideoPlayerEditor, IProcessSceneWithReport
    {
        public int callbackOrder => 10;

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            foreach (var root in scene.GetRootGameObjects())
                ProcessGameObject(root);
        }

        private void ProcessGameObject(GameObject go)
        {
            var relay = go.GetComponent<KinelButtonHoverRelay>();
            if (relay != null)
                SetupEventTrigger(relay);

            foreach (Transform child in go.transform)
                ProcessGameObject(child.gameObject);
        }

        private void SetupEventTrigger(KinelButtonHoverRelay relay)
        {
            var udonBehaviour = relay.GetComponent<UdonBehaviour>();
            if (udonBehaviour == null)
            {
                LogWarning($"[KinelButtonHoverRelay] UdonBehaviour not found on {relay.gameObject.name}");
                return;
            }

            var eventTrigger = relay.GetComponent<EventTrigger>();
            if (eventTrigger == null)
                eventTrigger = relay.gameObject.AddComponent<EventTrigger>();

            eventTrigger.triggers.RemoveAll(e =>
                e.eventID == EventTriggerType.PointerEnter || e.eventID == EventTriggerType.PointerExit);

            AddEntry(eventTrigger, EventTriggerType.PointerEnter, udonBehaviour, "OnHoverEnter");
            AddEntry(eventTrigger, EventTriggerType.PointerExit, udonBehaviour, "OnHoverExit");

            Log($"[KinelButtonHoverRelay] EventTrigger setup: {relay.gameObject.name}");
        }

        private void AddEntry(EventTrigger trigger, EventTriggerType type,
            UdonBehaviour udonBehaviour, string methodName)
        {
            var entry = new EventTrigger.Entry { eventID = type };
            UnityEventTools.AddStringPersistentListener(
                entry.callback,
                new UnityAction<string>(udonBehaviour.SendCustomEvent),
                methodName
            );
            trigger.triggers.Add(entry);
        }
    }
}
