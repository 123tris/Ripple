using UnityEngine;

namespace Ripple
{
    /// <summary>
    /// Declares a typed variable slot. Use with VariableInstancer to get per-GameObject isolated values.
    /// </summary>
    public abstract class VariableDeclaration : ScriptableObject
    {
        public abstract object GetDefaultValue();
        public abstract System.Type ValueType { get; }
    }

    public abstract class VariableDeclaration<T> : VariableDeclaration
    {
        [SerializeField] private T _initialValue;

        public T InitialValue => _initialValue;

        public override object GetDefaultValue() => _initialValue;
        public override System.Type ValueType => typeof(T);
    }
}
