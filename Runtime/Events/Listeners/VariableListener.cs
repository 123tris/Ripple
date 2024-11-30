using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UltEvents;
using UnityEngine;

namespace Ripple
{
    public class VariableListener<T> : MonoBehaviour
    {
        [SerializeField,HideReferenceObjectPicker] private VariableSO<T> variable;

        [SerializeField] UltEvent<T> onValueChanged;

        void OnEnable()
        {
            variable.OnValueChanged += OnValueChanged;
        }

        private void OnValueChanged(T v)
        {
            onValueChanged.Invoke(v);
        }

        void OnDisable()
        {
            variable.OnValueChanged -= OnValueChanged;
        }
    }
}