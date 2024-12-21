using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UltEvents;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ripple
{
    [InlineEditor]
    public class VariableSO<T> : ScriptableObject
    {
#if UNITY_EDITOR
        [SerializeField, TextArea, HideInInlineEditors]
        private string _developerNotes;
#endif

        [SerializeField, HideInInspector] private T _currentValue;

        [SerializeField, HideIf("@UnityEngine.Application.isPlaying")]
        private T _initialValue;

        private T _previousValue;

        [ShowInInspector, ShowIf("@UnityEngine.Application.isPlaying")]
        public T CurrentValue
        {
            get => _currentValue;
            set
            {
                _previousValue = _currentValue;
                if (Application.isPlaying)
                    OnValueChanged?.Invoke(value);
                _currentValue = value;
            }
        }

        private T PreviousValue => _previousValue ?? default;

        public UltEvent<T> OnValueChanged;

        // [ShowInInspector, ShowIf("@UnityEngine.Application.isPlaying")]
        // private Delegate[] ObjectsListeningToValueChanges => OnValueChanged?.GetInvocationList();

#if UNITY_EDITOR
        private void EditorApplicationOnplayModeStateChanged(UnityEditor.PlayModeStateChange playModeState)
        {
            if (playModeState == UnityEditor.PlayModeStateChange.EnteredPlayMode)
                ResetValue();
        }

        private void OnDisable()
        {
            UnityEditor.EditorApplication.playModeStateChanged -= EditorApplicationOnplayModeStateChanged;
        }
#endif

        private void OnEnable()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged += EditorApplicationOnplayModeStateChanged;
#endif
            ResetValue();
        }

        protected void ResetValue()
        {
            _currentValue = _initialValue;
            CurrentValue = _initialValue;
        }
    }

    public interface INumericalVariable<T>
    {
        public void Add(T value);
    }
}