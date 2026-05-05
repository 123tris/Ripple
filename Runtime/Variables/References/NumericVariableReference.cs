using System;
using UnityEngine;

namespace Ripple
{
    [Serializable]
    public sealed class NumericVariableReference
    {
        [SerializeField] private bool _useConstant = true;
        [SerializeField] private double _constantValue;
        [SerializeField] private ScriptableObject _variable;

        public bool UseConstant { get => _useConstant; set => _useConstant = value; }
        public double ConstantValue { get => _constantValue; set => _constantValue = value; }
        public ScriptableObject VariableAsset
        {
            get => _variable;
            set
            {
                if (value != null && value is not INumericVariable)
                {
                    Debug.LogWarning($"[Ripple] {value.name} is not an INumericVariable; rejected.", value);
                    return;
                }
                _variable = value;
            }
        }

        public NumericVariableReference() { }
        public NumericVariableReference(double constant) { _useConstant = true; _constantValue = constant; }

        public double Value
        {
            get
            {
                if (_useConstant) return _constantValue;
                if (_variable is INumericVariable n) return n.AsDouble;
                return 0;
            }
        }

        public Type ValueType
        {
            get
            {
                if (_useConstant) return typeof(double);
                if (_variable is INumericVariable n) return n.ValueType;
                return typeof(double);
            }
        }

        public IDisposable Subscribe(Action<double> handler)
        {
            if (_useConstant || _variable is not INumericVariable n) return EmptyDisposable.Instance;
            return n.SubscribeNumeric(handler);
        }

        public static implicit operator double(NumericVariableReference r) => r != null ? r.Value : 0;
        public static implicit operator float(NumericVariableReference r) => r != null ? (float)r.Value : 0f;
        public static implicit operator int(NumericVariableReference r) => r != null ? (int)r.Value : 0;

        private sealed class EmptyDisposable : IDisposable
        {
            public static readonly EmptyDisposable Instance = new EmptyDisposable();
            public void Dispose() { }
        }
    }
}
