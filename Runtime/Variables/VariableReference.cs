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

        [SerializeField, InlineEditor] private VariableSO<T> _variable;

        public VariableReference() { }

        public VariableReference(T value)
        {
            useConstant = true;
            _constantValue = value;
        }

        public T Value => useConstant ? _constantValue : _variable.CurrentValue;

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
    [Serializable] public class Vector3Reference : VariableReference<Vector3>
    {
        public Vector3Reference(Vector3 value) : base(value) { }

        public Vector3Reference() { }
    }
    [Serializable] public class TransformReference : VariableReference<Transform> { }
    [Serializable] public class GameObjectReference : VariableReference<GameObject> { }
}