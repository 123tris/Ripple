using UnityEngine;
using UnityEngine.Serialization;

namespace Ripple
{
    [AddComponentMenu("Ripple/Runtime Set Member")]
    public class RuntimeSetMember : MonoBehaviour
    {
        [SerializeField, FormerlySerializedAs("Set")] private GameObjectListSO set;

        private void OnEnable() => set.Add(gameObject);

        private void OnDisable() => set.Remove(gameObject);
    }
}
