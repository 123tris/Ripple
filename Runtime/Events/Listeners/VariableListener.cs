using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Ripple
{
    public class VariableListener<T> : MonoBehaviour, IRippleListener
    {
        [SerializeField, Required, HideReferenceObjectPicker] private VariableSO<T> variable;

        [SerializeField] private Signal<T> onValueChanged = new();

        public RippleStackTraceSO Target => variable;
        public Type ResponseValueType => typeof(T);

        private void OnEnable() => variable.AddListener(OnValueChanged);

        private void OnDisable() => variable.RemoveListener(OnValueChanged);

        private void OnValueChanged(T value) => onValueChanged.Invoke(value);
    }
}
