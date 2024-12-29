using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UltEvents;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ripple
{
    [InlineEditor]
    public class VariableSO<T> : RippleStackTraceSO
    {
        [SerializeField, HideInInspector] private protected T _currentValue;

        [SerializeField, HideInPlayMode] private T _initialValue;

        private T _previousValue;

        [ShowInInspector, HideInEditorMode]
        public T CurrentValue
        {
            get => _currentValue;
            set => SetCurrentValue(value);
        }

        private protected virtual void SetCurrentValue(T value)
        {
            _previousValue = _currentValue;
            if (Application.isPlaying)
            {
                LogInvoke(value);
                OnValueChanged?.Invoke(value);
            }
            _currentValue = value;
        }

        public T PreviousValue => _previousValue ?? _initialValue;

        public UltEvent<T> OnValueChanged;

#if UNITY_EDITOR
        private void EditorApplicationOnplayModeStateChanged(UnityEditor.PlayModeStateChange playModeState)
        {
            if (playModeState == UnityEditor.PlayModeStateChange.ExitingEditMode)
            {
                ResetValue();
                invokeStackTraces.Clear();
            }
        }

        private void OnDisable() => UnityEditor.EditorApplication.playModeStateChanged -= EditorApplicationOnplayModeStateChanged;
#endif

        private void OnEnable()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged += EditorApplicationOnplayModeStateChanged;
#endif
            ResetValue();
        }

        private void ResetValue()
        {
            _currentValue = _initialValue;
        }
    }
}