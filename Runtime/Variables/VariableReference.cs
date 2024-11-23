using System;
using UnityEngine;

public class VariableReference<T>
{
    public bool useConstant = true;

    [SerializeField] private T _constantValue;

    [SerializeField] private VariableSO<T> _variable;

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

[Serializable] public class BoolReference : VariableReference<bool> { }
[Serializable] public class FloatReference : VariableReference<float> { }
[Serializable] public class TransformReference : VariableReference<Transform> { }
[Serializable] public class GameObjectReference : VariableReference<GameObject> { }