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
            if (variable == null)
            {
                Debug.LogWarning($"{nameof(VariableListener<T>)} on {name} has no variable assigned.", this);
                return;
            }
            variable.OnValueChanged += OnValueChanged;
        }

        private void OnValueChanged(T v)
        {
            onValueChanged.Invoke(v);
        }

        void OnDisable()
        {
            if (variable == null)
                return;
            variable.OnValueChanged -= OnValueChanged;
        }
    }
}