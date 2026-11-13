using System;
using UnityEngine;
using UnityEngine.Events;

namespace Ripple
{
    [Serializable]
    public class Signal
    {
        [SerializeField]
        private UnityEvent response = new();

        [SerializeField, Tooltip("Ripple events raised alongside the response.")]
        private VoidEventSO[] raise = Array.Empty<VoidEventSO>();

        private Action listeners;

        public UnityEvent Response => response;

        public bool HasListeners => listeners != null || raise.Length > 0 || response.GetPersistentEventCount() > 0;

        public void Invoke()
        {
            response.Invoke();
            foreach (VoidEventSO gameEvent in raise)
                gameEvent.Raise();
            listeners?.Invoke();
        }

        public void AddListener(Action listener) => listeners += listener;

        public void RemoveListener(Action listener) => listeners -= listener;

        public void ClearListeners() => listeners = null;

        public static Signal operator +(Signal signal, Action listener)
        {
            signal.AddListener(listener);
            return signal;
        }

        public static Signal operator -(Signal signal, Action listener)
        {
            signal.RemoveListener(listener);
            return signal;
        }
    }

    [Serializable]
    public class Signal<T>
    {
        [SerializeField]
        private UnityEvent<T> response = new();

        [SerializeField, Tooltip("Ripple events raised alongside the response. An event of this type gets the value, a void event is raised without it.")]
        private GameEvent[] raise = Array.Empty<GameEvent>();

        private Action<T> listeners;

        public UnityEvent<T> Response => response;

        public bool HasListeners => listeners != null || raise.Length > 0 || response.GetPersistentEventCount() > 0;

        public void Invoke(T value)
        {
            response.Invoke(value);
            foreach (GameEvent gameEvent in raise)
                gameEvent.Raise(value);
            listeners?.Invoke(value);
        }

        public void AddListener(Action<T> listener) => listeners += listener;

        public void RemoveListener(Action<T> listener) => listeners -= listener;

        public void ClearListeners() => listeners = null;

        public static Signal<T> operator +(Signal<T> signal, Action<T> listener)
        {
            signal.AddListener(listener);
            return signal;
        }

        public static Signal<T> operator -(Signal<T> signal, Action<T> listener)
        {
            signal.RemoveListener(listener);
            return signal;
        }
    }
}
