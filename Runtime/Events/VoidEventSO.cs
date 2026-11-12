using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Ripple
{
    [RippleData]
    [CreateAssetMenu(menuName = Config.EventMenu + "Void")]
    public class VoidEventSO : GameEvent
    {
        [SerializeField, FormerlySerializedAs("gameEvent")] private Signal response = new();

        public override bool HasListeners => response.HasListeners;

        internal override UnityEventBase SerializedResponse => response.Response;

        public void Invoke(UnityEngine.Object sender = null)
        {
#if UNITY_EDITOR
            RecordInvoke(ResolveCaller(sender), sender, null);
#endif
            response.Invoke();
        }

        public void Raise() => Invoke();

        public void AddListener(Action method)
        {
            response.AddListener(method);
            RippleConnectionRegistry.RecordSubscribe(this, method);
        }

        public void RemoveListener(Action method)
        {
            response.RemoveListener(method);
            RippleConnectionRegistry.RecordUnsubscribe(this, method);
        }

        protected override void ClearListeners() => response.ClearListeners();
    }
}
