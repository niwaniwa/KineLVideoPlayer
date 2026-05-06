using System;

namespace Kinel.VideoPlayer.V3.Scripts.Attribute
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class KinelUIEventAttribute : System.Attribute
    {
        /// <summary>紐づけたいメソッド名</summary>
        public string CallBackMethodName { get; }

        /// <summary>どのUIイベントに紐づけるか</summary>
        public UIEventType EventType { get; }

        public KinelUIEventAttribute(string callBackMethodName, UIEventType eventType)
        {
            CallBackMethodName = callBackMethodName;
            EventType = eventType;
        }
    }
}