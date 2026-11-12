using System;
using UnityEngine;

namespace Ripple
{
    public class EventListener<T> : MonoBehaviour, IRippleListener
    {
        [SerializeField] private GameEvent<T> _event;

        [SerializeField] private Signal<T> _response = new();

        public RippleStackTraceSO Target => _event;
        public Type ResponseValueType => typeof(T);

        private void OnEnable() => _event.AddListener(OnEvent);

        private void OnDisable() => _event.RemoveListener(OnEvent);

        private void OnEvent(T value) => _response.Invoke(value);
    }
}
