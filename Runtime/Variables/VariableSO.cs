using System;
using Sirenix.OdinInspector;
using UltEvents;
using UnityEngine;

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

        public virtual void SetCurrentValue(T value, UnityEngine.Object sender = null)
        {
            _previousValue = _currentValue;
            _currentValue = value;
            if (Application.isPlaying)
            {
                LogInvoke(value, sender);
                OnValueChanged?.Invoke(value);
            }
        }

        public T PreviousValue => _previousValue ?? _initialValue;

        [HideInInlineEditors] public UltEvent<T> OnValueChanged;

        /// <summary>Subscribe to value changes. Routes through the connection registry so editor tooling can list listeners.</summary>
        public void AddListener(Action<T> method)
        {
            OnValueChanged += method;
            RippleConnectionRegistry.RecordSubscribe(this, method);
        }

        public void RemoveListener(Action<T> method)
        {
            OnValueChanged -= method;
            RippleConnectionRegistry.RecordUnsubscribe(this, method);
        }

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
                ClearInvokers();
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
