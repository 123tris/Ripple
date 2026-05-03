using UnityEngine;

public abstract class VariableDeclaration : ScriptableObject
{
    public abstract object GetInitialValueAsObject();
}

public abstract class VariableDeclaration<T> : VariableDeclaration
{
    [SerializeField] private T initialValue;

    public T InitialValue => initialValue;

    public override object GetInitialValueAsObject() => initialValue;
}