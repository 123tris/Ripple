using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Ripple
{
    [InlineEditor]
    public class VariableSO<T> : ScriptableObject
    {
#if UNITY_EDITOR
        [SerializeField, TextArea, HideInInlineEditors] private string _developerNotes;
#endif

        [SerializeField, HideInInspector]
        private T _currentValue;

        [SerializeField, HideIf("@UnityEngine.Application.isPlaying")]
        private T _initialValue;

        private T _previousValue;

        [ShowInInspector, ShowIf("@UnityEngine.Application.isPlaying")]
        public T CurrentValue
        {
            get => _currentValue;
            set {
                _previousValue = _currentValue;
                OnValueChanged?.Invoke(value);
                _currentValue = value;
            }
        }

        private T PreviousValue => _previousValue ?? default;

        public Action<T> OnValueChanged;

        [ShowInInspector, ShowIf("@UnityEngine.Application.isPlaying")]
        private Delegate[] ObjectsListeningToValueChanges => OnValueChanged?.GetInvocationList();

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void ResetValue()
        {
            foreach (var variable in _activeVariables)
            {
                variable._currentValue = variable._initialValue;
            }
        }

        private static HashSet<VariableSO<T>> _activeVariables = new();

        void OnDisable() => _activeVariables.Remove(this);
#endif
        
        void OnEnable()
        {
            #if UNITY_EDITOR
            _activeVariables.Add(this);
            #endif
            _currentValue = _initialValue;
            CurrentValue = _initialValue;
        }
    }

    public interface INumericalVariable<T>
    {
        public void Add(T value);
    }
}