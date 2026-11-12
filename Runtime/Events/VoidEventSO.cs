using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Ripple
{
    [RippleData]
    [CreateAssetMenu(menuName = Config.EventMenu + "Void")]
    public class VoidEventSO : GameEvent
    {
        [SerializeField] private Signal gameEvent = new();

        public override bool HasListeners => gameEvent.HasListeners;

        internal override UnityEventBase SerializedResponse => gameEvent.Response;

        [Button]
        public void Invoke(UnityEngine.Object sender = null)
        {
#if UNITY_EDITOR
            RecordInvoke(ResolveCaller(sender), sender, null);
#endif
            gameEvent.Invoke();
        }

        public void Raise() => Invoke();

        public void AddListener(Action method)
        {
            gameEvent.AddListener(method);
            RippleConnectionRegistry.RecordSubscribe(this, method);
        }

        public void RemoveListener(Action method)
        {
            gameEvent.RemoveListener(method);
            RippleConnectionRegistry.RecordUnsubscribe(this, method);
        }

        protected override void ClearListeners() => gameEvent.ClearListeners();
    }
}
