using System;
using UltEvents;
using UnityEngine;

namespace Ripple
{
    [AddComponentMenu(Config.EventListenerMenu + "Event Listener Void")]
    public class EventListenerVoid : MonoBehaviour, IRippleListener
    {
        [SerializeField]
        private VoidEventSO _event;

        [SerializeField]
        private UltEvent _response;

        public RippleStackTraceSO Target => _event;
        public Type ResponseValueType => typeof(void);

        void OnEnable()
        {
            if (_event != null)
                _event.AddListener(OnEvent);
        }

        void OnDisable()
        {
            if (_event != null)
                _event.RemoveListener(OnEvent);
        }

        private void OnEvent()
        {
            _response?.Invoke();
        }
    }
}
