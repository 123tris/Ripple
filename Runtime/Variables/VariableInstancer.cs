using System;
using UnityEngine;

namespace Ripple
{
    public abstract class VariableInstancer<T> : MonoBehaviour
    {
        [SerializeField] private VariableSO<T> _shared;
        [SerializeField] private T _initialOverride;

        private VariableSO<T> _local;

        public VariableSO<T> Effective => _local != null ? _local : _shared;

        protected virtual void Awake()
        {
            if (_shared == null) return;
            _local = ScriptableObject.Instantiate(_shared);
            _local.name = $"{_shared.name}__{gameObject.name}";
            _local.SetCurrentValue(_initialOverride);
        }

        protected virtual void OnDestroy()
        {
            if (_local != null) Destroy(_local);
            _local = null;
        }
    }

    public sealed class FloatVariableInstancer : VariableInstancer<float> { }
    public sealed class IntVariableInstancer : VariableInstancer<int> { }
    public sealed class BoolVariableInstancer : VariableInstancer<bool> { }
}
