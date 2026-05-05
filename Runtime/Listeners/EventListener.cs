using UnityEngine;
#if RIPPLE_ULTEVENTS
using UltEvents;
#else
using UnityEngine.Events;
#endif

namespace Ripple
{
    public class EventListener<T> : MonoBehaviour
    {
        [SerializeField] private GameEvent<T> _event;
#if RIPPLE_ULTEVENTS
        [SerializeField] private UltEvent<T> _response;
#else
        [SerializeField] private UnityEvent<T> _response;
#endif

        public GameEvent<T> Event { get => _event; set => Rebind(value); }

        protected virtual void OnEnable()
        {
            if (_event != null) _event.AddListener(OnEvent);
        }

        protected virtual void OnDisable()
        {
            if (_event != null) _event.RemoveListener(OnEvent);
        }

        private void Rebind(GameEvent<T> next)
        {
            if (_event == next) return;
            if (isActiveAndEnabled && _event != null) _event.RemoveListener(OnEvent);
            _event = next;
            if (isActiveAndEnabled && _event != null) _event.AddListener(OnEvent);
        }

        protected virtual void OnEvent(T value)
        {
            _response?.Invoke(value);
        }
    }

    public sealed class VoidEventListener : EventListener<Unit> { }
}
