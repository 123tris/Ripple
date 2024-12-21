using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Ripple
{
    [InlineEditor]
    public class VariableListSO<T> : ScriptableObject
    {
#if UNITY_EDITOR
        [SerializeField, TextArea, HideInInlineEditors] private string _developerNotes;
#endif

        [SerializeField, HideInInspector]
        private List<T> _currentValue;

        [SerializeField, HideIf("@UnityEngine.Application.isPlaying")]
        private List<T> _initialValue;

        [ShowInInspector, ShowIf("@UnityEngine.Application.isPlaying")]
        public List<T> CurrentValues
        {
            get => _currentValue;
        }

        public Action<T> OnValueAdded;
        public Action<T> OnValueRemoved;

        [ShowInInspector, ShowIf("@UnityEngine.Application.isPlaying")]
        private Delegate[] ObjectsListeningToValueAddition => OnValueAdded?.GetInvocationList();
        private Delegate[] ObjectsListeningToValueRemoval => OnValueRemoved?.GetInvocationList();

        void OnEnable()
        {
            _currentValue = new(_initialValue);
        }

        public void AddValue(T value)
        {
            _currentValue.Add(value);
            OnValueAdded?.Invoke(value);
        }

        public bool RemoveValue(T value)
        {
            if (!_currentValue.Contains(value))
            {
                return false;
            }
            _currentValue.Remove(value);
            OnValueRemoved?.Invoke(value);
            return true;
        }
    }
}
