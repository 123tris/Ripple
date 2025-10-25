using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UltEvents;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ripple
{
    [InlineEditor]
    public class VariableSO<T> : BaseVariable<T>
    {
        [SerializeField, HideInInspector] private protected T _currentValue;

        [SerializeField, HideInPlayMode] private T _initialValue;

        private T _previousValue;

        public override T CurrentValue => _currentValue;

#if UNITY_EDITOR
        [ShowInInspector, HideInEditorMode, LabelText("Value")]
        private T EditorValue
        {
            get => _currentValue;
            set => SetCurrentValue(value);
        }
#endif

        public virtual void SetCurrentValue(T value)
        {
            _previousValue = _currentValue;
            _currentValue = value;
            if (Application.isPlaying)
            {
                LogInvoke(value);
                OnValueChanged?.Invoke(value);
            }
        }

        public T PreviousValue => _previousValue ?? _initialValue;

        [HideInInlineEditors] public UltEvent<T> OnValueChanged;

        protected override void ResetValue()
        {
            _currentValue = _initialValue;
        }

        public override object Value => _currentValue;
    }

    public abstract class BaseVariable<T> : BaseVariable
    {
        public abstract T CurrentValue { get; }

        protected virtual void OnEnable()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged += EditorApplicationOnplayModeStateChanged;
#endif
            ResetValue();
        }
#if UNITY_EDITOR
        private void EditorApplicationOnplayModeStateChanged(UnityEditor.PlayModeStateChange playModeState)
        {
            if (playModeState == UnityEditor.PlayModeStateChange.ExitingEditMode)
            {
                ResetValue();
                invokeStackTraces.Clear();
            }
        }

        private void OnDisable() => UnityEditor.EditorApplication.playModeStateChanged -=
            EditorApplicationOnplayModeStateChanged;
#endif
        
        protected virtual void ResetValue()
        {
            
        }
    }

    public abstract class BaseVariable : RippleStackTraceSO
    {
        public abstract object Value { get; }
    }
}