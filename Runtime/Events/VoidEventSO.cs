using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Ripple
{
    [CreateAssetMenu(menuName = Config.EventMenu + "Void")]
    public class VoidEventSO : GameEvent
    {
        [SerializeField, FormerlySerializedAs("gameEvent")] private Signal response = new();

        public override bool HasListeners => base.HasListeners || response.HasListeners;

        internal override IEnumerable<UnityEventBase> SerializedResponses => new UnityEventBase[] { response.Response };

        public void Invoke(Object sender = null)
        {
#if UNITY_EDITOR
            RecordInvoke(ResolveCaller(sender), sender, null);
#endif
            response.Invoke();
            NotifyListeners();
        }

        public void Raise() => Invoke();
    }
}
