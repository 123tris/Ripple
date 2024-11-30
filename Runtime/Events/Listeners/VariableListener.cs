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

        void Awake()
        {
            variable.OnValueChanged += v => onValueChanged.Invoke(v);
        }
    }
}