using System;
using System.Collections.Generic;
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
        [NonSerialized] private protected T _currentValue;

        [SerializeField, PropertyOrder(-1), HideIf("@UnityEngine.Application.isPlaying || UsesSlider")] private T _initialValue;

        [SerializeField, Tooltip("Save the value to PlayerPrefs on change and restore it next session. Avoid on per-frame variables, every change writes.")]
        private bool _persist;

        [SerializeField, ShowIf(nameof(_persist)), Tooltip("PlayerPrefs key. Leave empty to use the asset name.")]
        private string _persistKey;

        [HideInInlineEditors] public Signal<T> OnValueChanged = new();

        private T _previousValue;

        public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;

        private string PersistKey => string.IsNullOrEmpty(_persistKey) ? name : _persistKey;

        private T StartValue => _persist && TryLoadSavedValue(out T saved) ? saved : _initialValue;

        private protected virtual bool UsesSlider => false;

        private protected T InitialValue
        {
            get => _initialValue;
            set => _initialValue = value;
        }

        [CreateProperty, ShowInInspector, PropertyOrder(-1), ShowIf("@UnityEngine.Application.isPlaying && !UsesSlider"), LabelText("Value")]
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

        [Button, BoxGroup("Debug", order: 1), HideInInlineEditors]
        public override void ResetValue() => SetCurrentValue(StartValue);

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

        protected override void Restore() => _currentValue = StartValue;

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
            if (!Application.isPlaying) Restore();
        }
    }

    public abstract class BaseVariable<T> : BaseVariable
    {
        public abstract T CurrentValue { get; set; }
    }

    public abstract class BaseVariable : RippleStackTraceSO
    {
        private static readonly List<BaseVariable> loaded = new();

        public abstract object Value { get; }

        public static void ResetAll()
        {
            foreach (BaseVariable variable in loaded.ToArray())
                variable.ResetValue();
        }

        public abstract void ResetValue();

        protected abstract void Restore();

        protected virtual void OnEnable()
        {
            loaded.Add(this);
#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
            Restore();
        }

        protected virtual void OnDisable()
        {
            loaded.Remove(this);
#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
#endif
        }

#if UNITY_EDITOR
        private void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange state)
        {
            if (state != UnityEditor.PlayModeStateChange.ExitingEditMode) return;
            Restore();
            ClearInvokers();
        }
#endif
    }
}
