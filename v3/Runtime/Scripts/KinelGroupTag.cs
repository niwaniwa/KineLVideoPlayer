using UnityEngine;

namespace Kinel.VideoPlayer.V3.Scripts
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Kinel/Group Tag")]
    public class KinelGroupTag : MonoBehaviour
    {
        [SerializeField] private KinelGroupConfig group;

        public KinelGroupConfig Group => group;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (group == null)
                Debug.LogWarning($"[KineL] KinelGroupTag on '{name}' has no KinelGroupConfig assigned.", this);
        }
#endif
    }
}
