using UnityEngine;

namespace Ripple
{
    public abstract class NumericalVariable<T> : VariableSO<T>
    {
        public bool _isClamped;
        public VariableReference<T> min;
        public VariableReference<T> max;

        public override void SetCurrentValue(T value, Object sender = null)
        {
            if (_isClamped)
                value = Clamp(value);
            base.SetCurrentValue(value, sender);
        }

        protected abstract T Clamp(T value);

        public abstract void Add(T value);

        public void Add(NumericalVariable<T> variable) => Add(variable.CurrentValue);
    }
}
