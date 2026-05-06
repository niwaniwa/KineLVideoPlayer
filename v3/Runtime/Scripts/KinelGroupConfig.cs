using UnityEngine;

namespace Kinel.VideoPlayer.V3.Scripts
{
    [CreateAssetMenu(menuName = "Kinel/Group Config", fileName = "NewKinelGroup")]
    public class KinelGroupConfig : ScriptableObject
    {
        [SerializeField] private string displayName;
        [SerializeField, TextArea] private string note;

        public string DisplayName => displayName;
        public string Note => note;
    }
}
