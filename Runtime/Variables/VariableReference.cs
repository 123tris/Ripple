using System;
using UnityEngine;

namespace Ripple
{
    public abstract class VariableReferenceBase { }

    [Serializable]
    public class VariableReference<T> : VariableReferenceBase
    {
        [SerializeField] private bool _useConstant = true;
        [SerializeField] private T _constantValue;
        [SerializeField] private VariableSO<T> _variable;

        public bool UseConstant { get => _useConstant; set => _useConstant = value; }
        public VariableSO<T> Variable { get => _variable; set => _variable = value; }
        public T ConstantValue { get => _constantValue; set => _constantValue = value; }

        public VariableReference() { }
        public VariableReference(T constant) { _useConstant = true; _constantValue = constant; }
        public VariableReference(VariableSO<T> variable) { _useConstant = false; _variable = variable; }

        public T Value
        {
            get
            {
                if (_useConstant) return _constantValue;
                return _variable != null ? _variable.CurrentValue : default;
            }
        }

        public IDisposable Subscribe(Action<T> handler)
        {
            if (_useConstant || _variable == null) return EmptyDisposable.Instance;
            return _variable.Subscribe(handler);
        }

        public static implicit operator T(VariableReference<T> r) => r != null ? r.Value : default;

        private sealed class EmptyDisposable : IDisposable
        {
            public static readonly EmptyDisposable Instance = new EmptyDisposable();
            public void Dispose() { }
        }
    }
}
