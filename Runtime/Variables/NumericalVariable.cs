using Sirenix.OdinInspector;
using UnityEngine;

namespace Ripple
{
    public abstract class NumericalVariable<T> : VariableSO<T>
    {
        [SerializeField] protected bool _isClamped;

        [SerializeField, ShowIf(nameof(_isClamped)), HorizontalGroup("clamping")]
        protected VariableReference<T> min;

        [SerializeField, ShowIf(nameof(_isClamped)), HorizontalGroup("clamping")]
        protected VariableReference<T> max;

        private protected override void SetCurrentValue(T value)
        {
            if (_isClamped)
                value = Clamp(value);
            base.SetCurrentValue(value);
        }

        protected abstract T Clamp(T value);

        public void Add(T value)
        {
            CurrentValue += (dynamic)value; //Kind of dangerous code as the cast will make the compiler no longer check ahead of time if the operator is actually implemented and thus for certain types can incur a runtime exception
        }
    }
}