using System;
using UltEvents;
using UnityEngine;

namespace Ripple
{
    public class EventListener<T> : MonoBehaviour, IRippleListener
    {
        [SerializeField]
        private GameEvent<T> _event;

        [SerializeField]
        private UltEvent<T> _response;

        public RippleStackTraceSO Target => _event;
        public Type ResponseValueType => typeof(T);

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

        private void OnEvent(T value)
        {
            _response?.Invoke(value);
        }
    }
}
