using System;
using Sirenix.OdinInspector;
using UltEvents;
using UnityEngine;

namespace Ripple
{
    public class VariableListener<T> : MonoBehaviour, IRippleListener
    {
        [SerializeField, HideReferenceObjectPicker] private VariableSO<T> variable;

        [SerializeField] UltEvent<T> onValueChanged;

        public RippleStackTraceSO Target => variable;
        public Type ResponseValueType => typeof(T);

        void OnEnable()
        {
            if (variable != null)
                variable.AddListener(OnValueChanged);
        }

        private void OnValueChanged(T v)
        {
            onValueChanged?.Invoke(v);
        }

        void OnDisable()
        {
            if (variable != null)
                variable.RemoveListener(OnValueChanged);
        }
    }
}
