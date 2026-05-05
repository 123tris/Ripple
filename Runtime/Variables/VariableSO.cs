using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Ripple
{
    public abstract class BaseVariable : RippleStackTraceSO, IChannel
    {
        public string Name => name;
        public abstract Type PayloadType { get; }
        public abstract int SubscriberCount { get; }
        public abstract object BoxedValue { get; }
        public abstract void ResetToInitial();
    }

    public abstract class BaseVariable<T> : BaseVariable, IChannel<T>
    {
        public abstract T CurrentValue { get; }
        public abstract T PreviousValue { get; }

        public abstract IDisposable Subscribe(Action<T> handler);
        public abstract void Unsubscribe(Action<T> handler);
        public abstract void Invoke(T value,
            [CallerMemberName] string callerMember = null,
            [CallerFilePath] string callerFile = null,
            [CallerLineNumber] int callerLine = 0);
    }

    public class VariableSO<T> : BaseVariable<T>
    {
        [SerializeField] protected T _initialValue;
        [SerializeField, HideInInspector] protected T _currentValue;
        [SerializeReference] private List<IValidator<T>> _validators = new List<IValidator<T>>();

        protected T _previousValue;
        private readonly Dispatcher<T> _dispatcher = new Dispatcher<T>();

        public override T CurrentValue => _currentValue;
        public override T PreviousValue => _previousValue;
        public override Type PayloadType => typeof(T);
        public override int SubscriberCount => _dispatcher.Count;
        public override object BoxedValue => _currentValue;

        public IReadOnlyList<IValidator<T>> Validators => _validators;
        public void AddValidator(IValidator<T> v) => _validators.Add(v);

        protected virtual void OnEnable()
        {
            hideFlags = HideFlags.DontUnloadUnusedAsset;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
            ResetToInitial();
        }

#if UNITY_EDITOR
        protected virtual void OnDisable()
        {
            UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange state)
        {
            if (state == UnityEditor.PlayModeStateChange.ExitingEditMode)
            {
                ResetToInitial();
                ClearDebug();
            }
        }
#endif

        public override void ResetToInitial()
        {
            _previousValue = _currentValue;
            _currentValue = _initialValue;
        }

        public virtual void SetCurrentValue(T value,
            [CallerMemberName] string callerMember = null,
            [CallerFilePath] string callerFile = null,
            [CallerLineNumber] int callerLine = 0)
        {
            T candidate = value;
            for (int i = 0; i < _validators.Count; i++)
            {
                var v = _validators[i];
                if (v == null) continue;
                if (!v.Validate(candidate, out var corrected)) return;
                candidate = corrected;
            }

            _previousValue = _currentValue;
            _currentValue = candidate;

            if (!Application.isPlaying) return;

            LogInvoke(candidate, callerMember, callerFile, callerLine);
            using (RippleTraceContext.PushFrame(this, candidate, callerMember, callerFile, callerLine))
            {
                _dispatcher.Invoke(candidate);
            }
        }

        public override void Invoke(T value,
            [CallerMemberName] string callerMember = null,
            [CallerFilePath] string callerFile = null,
            [CallerLineNumber] int callerLine = 0)
            => SetCurrentValue(value, callerMember, callerFile, callerLine);

        public override IDisposable Subscribe(Action<T> handler)
        {
            _dispatcher.Add(handler);
            return new Subscription(this, handler);
        }

        public override void Unsubscribe(Action<T> handler) => _dispatcher.Remove(handler);

#if UNITY_EDITOR
        public Action<T>[] GetSubscribersDebug() => _dispatcher.Snapshot();
#endif

        public static implicit operator T(VariableSO<T> v) => v != null ? v._currentValue : default;

        private sealed class Subscription : IDisposable
        {
            private VariableSO<T> _v;
            private Action<T> _h;
            public Subscription(VariableSO<T> v, Action<T> h) { _v = v; _h = h; }
            public void Dispose()
            {
                if (_v == null) return;
                _v._dispatcher.Remove(_h);
                _v = null; _h = null;
            }
        }
    }
}
