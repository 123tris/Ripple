using System;
using Sirenix.OdinInspector;
using UnityEngine;
#if ULTEVENTS
using UltEvents;
#else
using UnityEngine.Events;
#endif

namespace Ripple
{
    [RippleData]
    [InlineEditor]
    public class VariableSO<T> : BaseVariable<T>, IVariable<T>
    {
        [SerializeField, HideInInspector] private protected T _currentValue;

        [SerializeField, HideInPlayMode] private T _initialValue;

        private T _previousValue;
        private bool _hasPreviousValue;

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
            SetCurrentValue(value, null);
        }

        public virtual void SetCurrentValue(T value, UnityEngine.Object invoker)
        {
            _previousValue = _currentValue;
            _hasPreviousValue = true;
            _currentValue = value;
            if (Application.isPlaying)
            {
                LogInvoke(value, invoker);
                try
                {
                    OnValueChanged?.Invoke(value);
                }
                catch (Exception exception)
                {
#if UNITY_EDITOR
                    Logger.Log(new LogMetadata
                    {
                        Message = $"Exception while changing variable <color=yellow>{name}</color> to <color=green>{value}</color>",
                        Context = this,
                        Invoker = invoker,
                        Caller = exception.TargetSite?.Name ?? "unknown",
                        FullStackTrace = exception.StackTrace ?? string.Empty,
                        HasException = true,
                        ExceptionDetails = exception.ToString(),
                        SourceKind = LogSourceKind.VariableChange
                    });
#endif
                    throw;
                }
            }
        }

        public T PreviousValue => _hasPreviousValue ? _previousValue : _initialValue;

        [HideInInlineEditors]
#if ULTEVENTS
        public UltEvent<T> OnValueChanged;
#else
        public UnityEvent<T> OnValueChanged;
#endif

        protected override void ResetValue()
        {
            _currentValue = _initialValue;
            _hasPreviousValue = false;
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