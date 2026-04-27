using Sirenix.OdinInspector;
using UnityEngine;

namespace Ripple
{
    public abstract class NumericalVariable<T> : VariableSO<T> //TODO: Remove Odin attributes and implement dedicated editor behaviour in separate editor script
    {
        public bool _isClamped;

        [SerializeField, ShowIf(nameof(_isClamped)), HorizontalGroup("clamping")]
        public VariableReference<T> min;

        [SerializeField, ShowIf(nameof(_isClamped)), HorizontalGroup("clamping")]
        public VariableReference<T> max;

#if UNITY_EDITOR
        [ShowInInspector, HideInPlayMode, LabelText("Initial Value")]
        [ShowIf("@isClamped && !UnityEditor.EditorApplication.isPlaying")]
        [PropertyRange("@min.Value", "@max.Value")]
        [PropertyOrder(-1)]
        private T InitialValueSlider { get => _initialValue; set => _initialValue = value; }

        [ShowInInspector, HideInEditorMode, LabelText("Value")]
        [ShowIf("@isClamped && UnityEditor.EditorApplication.isPlaying")]
        [PropertyRange("@min.Value", "@max.Value")]
        [PropertyOrder(-1)]
        private T EditorValueSlider
        {
            get => _currentValue;
            set => SetCurrentValue(value);
        }
#endif

        public override void SetCurrentValue(T value)
        {
            SetCurrentValue(value, null);
        }

        public override void SetCurrentValue(T value, Object context)
        {
            if (_isClamped)
                value = Clamp(value);
            base.SetCurrentValue(value, context);
        }

        protected abstract T Clamp(T value);

        public virtual void Add(T value)
        {
            SetCurrentValue(CurrentValue + (dynamic)value); //Kind of dangerous code as the cast will make the compiler no longer check ahead of time if the operator is actually implemented and thus for certain types can incur a runtime exception
        }

        public void Add(NumericalVariable<T> variable)
        {
            Add(variable.CurrentValue);
        }
    }
}