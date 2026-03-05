using Sirenix.OdinInspector;
using System.Collections.Generic;
using UltEvents;
using UnityEngine;

namespace Ripple
{
    [InlineEditor]
    public class VariableSO<T> : BaseVariable<T> //TODO: Remove Odin attributes and implement dedicated editor behaviour in separate editor script
    {
        private protected T _currentValue;

        [SerializeField, HideInPlayMode]
        [HideIf("@isClamped || UnityEditor.EditorApplication.isPlaying")]
        private protected T _initialValue;

        private T _previousValue;

        public override T CurrentValue => _currentValue;

#if UNITY_EDITOR
        protected bool isClamped
        {
            get
            {
                if (this is NumericalVariable<T> nv)
                    return nv._isClamped;

                return false;
            }
        }

        [ShowInInspector, LabelText("Value")]
        [HideIf("@isClamped || !UnityEditor.EditorApplication.isPlaying")]
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

#if UNITY_EDITOR
        private void EditorApplicationOnplayModeStateChanged(UnityEditor.PlayModeStateChange playModeState)
        {
            if (playModeState == UnityEditor.PlayModeStateChange.ExitingEditMode)
            {
                ResetValue();
                stackTrace.Clear();
            }
        }
#endif

        protected override void OnDisable()
        {
            base.OnDisable();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged -=
            EditorApplicationOnplayModeStateChanged;
#endif
        }

        protected override void OnEnable()
        {
            base.OnEnable();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged += EditorApplicationOnplayModeStateChanged;
#endif
            ResetValue();
        }

        public override void ResetValue()
        {
            _currentValue = _initialValue;
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }

    public abstract class BaseVariable<T> : BaseVariable
    {
        public abstract T CurrentValue { get; }
    }

    public abstract class BaseVariable : RippleStackTraceSO
    {
        protected static List<BaseVariable> instances = new();

        protected virtual void OnEnable()
        {
            instances.Add(this);
        }

        protected virtual void OnDisable()
        {
            instances.Remove(this);
        }

        public abstract void ResetValue();

        public static void ResetAllVariables()
        {
            foreach (var variable in instances)
            {
                variable.ResetValue();
            }
        }
    }
}