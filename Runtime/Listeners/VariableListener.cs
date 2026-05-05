using UnityEngine;
#if RIPPLE_ULTEVENTS
using UltEvents;
#else
using UnityEngine.Events;
#endif

namespace Ripple
{
    public class VariableListener<T> : MonoBehaviour
    {
        [SerializeField] private VariableSO<T> _variable;
#if RIPPLE_ULTEVENTS
        [SerializeField] private UltEvent<T> _response;
#else
        [SerializeField] private UnityEvent<T> _response;
#endif

        [SerializeField] private bool _invokeOnEnable = false;

        public VariableSO<T> Variable { get => _variable; set => Rebind(value); }

        private System.IDisposable _subscription;

        protected virtual void OnEnable()
        {
            if (_variable == null) return;
            _subscription = _variable.Subscribe(OnChanged);
            if (_invokeOnEnable) OnChanged(_variable.CurrentValue);
        }

        protected virtual void OnDisable()
        {
            _subscription?.Dispose();
            _subscription = null;
        }

        private void Rebind(VariableSO<T> next)
        {
            if (_variable == next) return;
            if (isActiveAndEnabled) OnDisable();
            _variable = next;
            if (isActiveAndEnabled) OnEnable();
        }

        protected virtual void OnChanged(T value)
        {
            _response?.Invoke(value);
        }
    }
}
