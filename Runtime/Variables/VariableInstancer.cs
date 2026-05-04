using System.Collections.Generic;
using UnityEngine;

namespace Ripple
{
    /// <summary>
    /// Provides per-GameObject isolated variable values based on shared VariableDeclaration assets.
    /// Values are created from the declaration's initial value on Awake and never mutate the asset.
    /// </summary>
    public class VariableInstancer : MonoBehaviour
    {
        private readonly Dictionary<VariableDeclaration, object> _values = new();

        private void Awake()
        {
            _values.Clear();
        }

        public T GetValue<T>(VariableDeclaration<T> declaration)
        {
            if (declaration == null)
            {
                Debug.LogWarning($"{nameof(VariableInstancer)} on {name}: declaration is null.", this);
                return default;
            }

            if (_values.TryGetValue(declaration, out var boxed))
                return (T)boxed;

            var initial = declaration.InitialValue;
            _values[declaration] = initial;
            return initial;
        }

        public void SetValue<T>(VariableDeclaration<T> declaration, T value)
        {
            if (declaration == null)
            {
                Debug.LogWarning($"{nameof(VariableInstancer)} on {name}: declaration is null.", this);
                return;
            }

            _values[declaration] = value;
        }

        public bool HasValue(VariableDeclaration declaration) => _values.ContainsKey(declaration);

        public void ResetValue(VariableDeclaration declaration)
        {
            if (declaration == null) return;
            _values[declaration] = declaration.GetDefaultValue();
        }

        public void ResetAll()
        {
            _values.Clear();
        }
    }
}
