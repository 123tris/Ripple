using System;
using UnityEngine;

namespace Ripple
{
    [AddComponentMenu(Config.EventListenerMenu + "Event Listener Void")]
    public class EventListenerVoid : MonoBehaviour, IRippleListener
    {
        [SerializeField, Tooltip("Any event. The value of a typed event is ignored.")]
        private GameEvent _event;

        [SerializeField] private Signal _response = new();

        public RippleStackTraceSO Target => _event;
        public Type ResponseValueType => typeof(void);

        private void OnEnable() => _event.AddListener(OnEvent);

        private void OnDisable() => _event.RemoveListener(OnEvent);

        private void OnEvent() => _response.Invoke();
    }
}
