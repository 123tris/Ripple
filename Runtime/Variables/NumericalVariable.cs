using Sirenix.OdinInspector;
using UnityEngine;

namespace Ripple
{
    public abstract class NumericalVariable<T> : VariableSO<T>
    {
        public bool _isClamped;

        [SerializeField, ShowIf(nameof(_isClamped)), HorizontalGroup("clamping")]
        public VariableReference<T> min;

        [SerializeField, ShowIf(nameof(_isClamped)), HorizontalGroup("clamping")]
        public VariableReference<T> max;

        private protected override bool UsesSlider => _isClamped;

        [ShowInInspector, ShowIf(nameof(_isClamped)), PropertyOrder(-1), PropertyRange("@min.Value", "@max.Value")]
        [LabelText("@UnityEngine.Application.isPlaying ? \"Value\" : \"Initial Value\"")]
        private T ClampedValue
        {
            get => Application.isPlaying ? CurrentValue : InitialValue;
            set
            {
                if (Application.isPlaying) CurrentValue = value;
                else InitialValue = Clamp(value);
            }
        }

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
