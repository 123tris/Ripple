using System;
using Sirenix.OdinInspector;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace Ripple
{
    [InlineEditor]
    public class VariableSO<T> : BaseVariable<T>, INotifyBindablePropertyChanged
    {
        [SerializeField, HideInInspector, DontCreateProperty] private protected T _currentValue;

        [SerializeField, HideInPlayMode] private T _initialValue;

        [SerializeField, Tooltip("Save the value to PlayerPrefs on change and restore it next session. Avoid on per-frame variables, every change writes.")]
        private bool _persist;

        [SerializeField, ShowIf(nameof(_persist)), Tooltip("PlayerPrefs key. Leave empty to use the asset name.")]
        private string _persistKey;

        [HideInInlineEditors] public Signal<T> OnValueChanged = new();

        private T _previousValue;

        public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;

        private string PersistKey => string.IsNullOrEmpty(_persistKey) ? name : _persistKey;

        [CreateProperty, ShowInInspector, HideInEditorMode, LabelText("Value")]
        public override T CurrentValue
        {
            get => _currentValue;
            set => SetCurrentValue(value);
        }

        public T PreviousValue => _previousValue ?? _initialValue;

        public override object Value => _currentValue;

        internal override UnityEventBase SerializedResponse => OnValueChanged.Response;

        public virtual void SetCurrentValue(T value, UnityEngine.Object sender = null)
        {
            _previousValue = _currentValue;
            _currentValue = value;
            if (_persist) SaveCurrentValue();
            propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(nameof(CurrentValue)));
            if (!Application.isPlaying) return;
            LogInvoke(value, sender);
            OnValueChanged.Invoke(value);
        }

        public void AddListener(Action<T> method)
        {
            OnValueChanged.AddListener(method);
            RippleConnectionRegistry.RecordSubscribe(this, method);
        }

        public void RemoveListener(Action<T> method)
        {
            OnValueChanged.RemoveListener(method);
            RippleConnectionRegistry.RecordUnsubscribe(this, method);
        }

        protected override void ResetValue()
        {
            _currentValue = _initialValue;
            if (_persist && TryLoadSavedValue(out var saved)) _currentValue = saved;
        }

        [Serializable] private struct PersistBox { public T Value; }

        private static bool IsUnityObject => typeof(UnityEngine.Object).IsAssignableFrom(typeof(T));

        private void SaveCurrentValue()
        {
            if (IsUnityObject) return;
            PlayerPrefs.SetString(PersistKey, JsonUtility.ToJson(new PersistBox { Value = _currentValue }));
        }

        private bool TryLoadSavedValue(out T value)
        {
            value = default;
            if (IsUnityObject || !PlayerPrefs.HasKey(PersistKey)) return false;
            value = JsonUtility.FromJson<PersistBox>(PlayerPrefs.GetString(PersistKey)).Value;
            return true;
        }

        [Button("Clear Saved Value"), ShowIf(nameof(_persist))]
        private void ClearSavedValue()
        {
            PlayerPrefs.DeleteKey(PersistKey);
            if (!Application.isPlaying) ResetValue();
        }
    }

    public abstract class BaseVariable<T> : BaseVariable
    {
        public abstract T CurrentValue { get; set; }

        protected virtual void OnEnable()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
            ResetValue();
        }

#if UNITY_EDITOR
        private void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange state)
        {
            if (state != UnityEditor.PlayModeStateChange.ExitingEditMode) return;
            ResetValue();
            ClearInvokers();
        }

        private void OnDisable() => UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
#endif

        protected virtual void ResetValue() { }
    }

    public abstract class BaseVariable : RippleStackTraceSO
    {
        public abstract object Value { get; }
    }
}
