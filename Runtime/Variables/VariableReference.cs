using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Ripple
{
    [Serializable]
    public class VariableReference<T> : VariableReferenceBase
    {
        public bool useConstant = true;

        [SerializeField] private T _constantValue;

        [SerializeField, InlineEditor] private BaseVariable<T> _variable;

        public VariableReference() { }

        public VariableReference(T value)
        {
            useConstant = true;
            _constantValue = value;
        }

        public T Value
        {
            get
            {
                if (useConstant)
                    return _constantValue;
                if (_variable)
                    return _variable.CurrentValue;
                return default;
            }
        }

        public static implicit operator T(VariableReference<T> reference)
        {
            return reference.Value;
        }
    }

    public class VariableReferenceBase { }

    [Serializable]
    public class BoolReference : VariableReference<bool>
    {
        public BoolReference(bool val) : base(val) {}

        public BoolReference() { }
    }

    [Serializable]
    public class FloatReference : VariableReference<float>
    {
        public FloatReference(float val) : base(val) {}

        public FloatReference() { }
    }
    [Serializable] public class IntReference : VariableReference<int>
    {
        public IntReference(int val) : base(val) {}

        public IntReference() { }
    }

    public interface INumericVariable
    {
        float GetNumericValue();
        void SetNumericValue(float value, NumericWriteMode writeMode = NumericWriteMode.RoundToNearest, UnityEngine.Object invoker = null);
    }

    public enum NumericWriteMode
    {
        RoundToNearest,
        Floor,
        Ceil,
        Truncate
    }

    [Serializable]
    public class NumericVariableReference : VariableReferenceBase
    {
        public bool useConstant = true;
        [SerializeField] private float _constantValue;
        [SerializeField] private BaseVariable _variable;
        [SerializeField] private NumericWriteMode _writeMode = NumericWriteMode.RoundToNearest;

        public float GetValue()
        {
            if (useConstant)
                return _constantValue;

            return _variable is INumericVariable numericVariable ? numericVariable.GetNumericValue() : 0f;
        }

        public void SetValue(float value, UnityEngine.Object invoker = null)
        {
            if (useConstant)
            {
                _constantValue = value;
                return;
            }

            if (_variable is INumericVariable numericVariable)
                numericVariable.SetNumericValue(value, _writeMode, invoker);
        }

        public float Value
        {
            get => GetValue();
            set => SetValue(value);
        }
    }
    [Serializable] public class Vector3Reference : VariableReference<Vector3>
    {
        public Vector3Reference(Vector3 value) : base(value) { }

        public Vector3Reference() { }
    }
    [Serializable] public class GameObjectReference : VariableReference<GameObject> { }

    [Serializable]
    public class Vector2Reference : VariableReference<Vector2>
    {
        public Vector2Reference(Vector2 value) : base(value) { }
        public Vector2Reference() { }
    }

    [Serializable]
    public class StringReference : VariableReference<string>
    {
        public StringReference(string value) : base(value) { }
        public StringReference() { }
    }

    [Serializable]
    public class ColorReference : VariableReference<UnityEngine.Color>
    {
        public ColorReference(UnityEngine.Color value) : base(value) { }
        public ColorReference() { }
    }
}