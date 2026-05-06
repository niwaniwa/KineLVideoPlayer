using System;
using System.Reflection;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using VRC.SDK3.Components;
using VRC.Udon;
using Kinel.VideoPlayer.V3.Scripts.Attribute;
using UnityEngine.EventSystems;

namespace Kinel.VideoPlayer.V3.Editor
{
    public class KinelUIEventProcessor : BaseKinelVideoPlayerEditor, IProcessSceneWithReport
    {
        public const string DelegateMethod = "SendCustomEvent";

        public int callbackOrder
        {
            get => 0;
        }

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            foreach (var root in scene.GetRootGameObjects())
                ProcessGameObject(root);
        }

        private void ProcessGameObject(GameObject go)
        {
            foreach (var behaviour in go.GetComponents<UdonSharpBehaviour>())
                ProcessComponent(behaviour);

            foreach (Transform child in go.transform)
                ProcessGameObject(child.gameObject);
        }

        private void ProcessComponent(UdonSharpBehaviour comp)
        {
            if (comp == null) return;

            var type = comp.GetType();
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            foreach (var field in type.GetFields(flags))
            {
                var attributes = field.GetCustomAttributes<KinelUIEventAttribute>();
                foreach (var attribute in attributes)
                {
                    if (attribute != null)
                        TryBindEvent(comp, field.GetValue(comp), attribute);
                }
            }

            foreach (var prop in type.GetProperties(flags))
            {
                var attributes = prop.GetCustomAttributes<KinelUIEventAttribute>();
                foreach (var attribute in attributes)
                {
                    if (attribute != null && prop.CanRead)
                        TryBindEvent(comp, prop.GetValue(comp), attribute);
                }
            }
        }

        private void TryBindEvent(UdonSharpBehaviour comp, object uiObject, KinelUIEventAttribute attr)
        {
            if (uiObject == null)
            {
                LogWarning($"UI object is null for {attr.CallBackMethodName} on {comp.name}");
                return;
            }

            var method = comp.GetType().GetMethod(attr.CallBackMethodName,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            var delegateMethod = comp.GetType().GetMethod(DelegateMethod,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            if (method == null || delegateMethod == null)
            {
                LogWarning($"Method '{attr.CallBackMethodName}' not found on {comp.name}");
                return;
            }

            var udonBehaviour = comp.GetComponent<UdonBehaviour>();
            if (udonBehaviour == null)
            {
                LogWarning($"UdonBehaviour not found on {comp.name}");
                return;
            }

            switch (attr.EventType)
            {
                case UIEventType.ButtonClick:
                    if (uiObject is Button button)
                    {
                        UnityEventTools.AddStringPersistentListener(
                            button.onClick,
                            (UnityAction<string>)Delegate.CreateDelegate(
                                typeof(UnityAction<string>),
                                udonBehaviour,
                                "SendCustomEvent"
                            ),
                            attr.CallBackMethodName
                        );
                    }

                    break;

                case UIEventType.ToggleChanged:
                    if (uiObject is Toggle toggle)
                    {
                        UnityEventTools.AddStringPersistentListener(
                            toggle.onValueChanged,
                            (UnityAction<string>)Delegate.CreateDelegate(
                                typeof(UnityAction<string>),
                                udonBehaviour,
                                "SendCustomEvent"
                            ),
                            attr.CallBackMethodName
                        );
                    }

                    break;

                case UIEventType.SliderChanged:
                    if (uiObject is Slider sliderChanged)
                    {
                        UnityEventTools.AddStringPersistentListener(
                            sliderChanged.onValueChanged,
                            (UnityAction<string>)Delegate.CreateDelegate(
                                typeof(UnityAction<string>),
                                udonBehaviour,
                                "SendCustomEvent"
                            ),
                            attr.CallBackMethodName
                        );
                    }

                    break;
                case UIEventType.EventTriggerDrag:
                    AddEventTrigger(uiObject as EventTrigger, EventTriggerType.Drag, udonBehaviour,
                        attr.CallBackMethodName);

                    break;

                case UIEventType.EventTriggerEndDrag:
                    AddEventTrigger(uiObject as EventTrigger, EventTriggerType.EndDrag, udonBehaviour,
                        attr.CallBackMethodName);

                    break;

                case UIEventType.InputChanged:
                    if (uiObject is VRCUrlInputField input)
                    {
                        UnityEventTools.AddStringPersistentListener(
                            input.onValueChanged,
                            (UnityAction<string>)Delegate.CreateDelegate(
                                typeof(UnityAction<string>),
                                udonBehaviour,
                                "SendCustomEvent"
                            ),
                            attr.CallBackMethodName
                        );
                    }

                    break;
                case UIEventType.EndEdit:
                    if (uiObject is VRCUrlInputField inputField)
                    {
                        UnityEventTools.AddStringPersistentListener(
                            inputField.onEndEdit,
                            (UnityAction<string>)Delegate.CreateDelegate(
                                typeof(UnityAction<string>),
                                udonBehaviour,
                                "SendCustomEvent"
                            ),
                            attr.CallBackMethodName
                        );
                    }

                    break;
                default:
                    LogWarning($"Event type {attr.EventType} is not supported");
                    break;
            }
        }

        private void AddEventTrigger(EventTrigger eventTrigger, EventTriggerType triggerType,
            UdonBehaviour udonBehaviour, string methodName)
        {
            if (eventTrigger == null) return;

            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = triggerType;

            UnityEventTools.AddStringPersistentListener(
                entry.callback,
                new UnityAction<string>(udonBehaviour.SendCustomEvent),
                methodName
            );

            eventTrigger.triggers.Add(entry);
        }
    }
}