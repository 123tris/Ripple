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

        [SerializeField, Tooltip("Save the value to PlayerPrefs on change and restore it next session (settings, high scores). Avoid on per-frame variables — every change writes.")]
        private bool _persist;

        [SerializeField, ShowIf(nameof(_persist)), Tooltip("PlayerPrefs key. Leave empty to use the asset name.")]
        private string _persistKey;

        private T _previousValue;

        private string PersistKey => string.IsNullOrEmpty(_persistKey) ? name : _persistKey;

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
            if (_persist) SaveCurrentValue();
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
            if (_persist && TryLoadSavedValue(out var saved)) _currentValue = saved;
        }

        // PlayerPrefs can't hold Unity Object references, so persistence quietly skips them.
        // Values are boxed through JsonUtility so every serializable T shares one code path.
        [Serializable] private struct PersistBox { public T Value; }

        private void SaveCurrentValue()
        {
            if (typeof(UnityEngine.Object).IsAssignableFrom(typeof(T))) return;
            PlayerPrefs.SetString(PersistKey, JsonUtility.ToJson(new PersistBox { Value = _currentValue }));
        }

        private bool TryLoadSavedValue(out T value)
        {
            value = default;
            if (typeof(UnityEngine.Object).IsAssignableFrom(typeof(T))) return false;
            if (!PlayerPrefs.HasKey(PersistKey)) return false;
            value = JsonUtility.FromJson<PersistBox>(PlayerPrefs.GetString(PersistKey)).Value;
            return true;
        }

        [Button("Clear Saved Value"), ShowIf(nameof(_persist))]
        private void ClearSavedValue()
        {
            PlayerPrefs.DeleteKey(PersistKey);
            if (!Application.isPlaying) ResetValue();
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
