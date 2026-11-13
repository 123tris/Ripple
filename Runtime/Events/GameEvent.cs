using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Ripple
{
    public class GameEvent<T> : GameEvent
    {
        [SerializeField] private Signal<T> response = new();

        public override bool HasListeners => base.HasListeners || response.HasListeners;

        internal override IEnumerable<UnityEventBase> SerializedResponses => new UnityEventBase[] { response.Response };

        public virtual void Invoke(T parameter, UnityEngine.Object sender = null)
        {
            LogInvoke(parameter, sender);
            response.Invoke(parameter);
            NotifyListeners();
        }

        public void Invoke(VariableSO<T> parameter) => Invoke(parameter.CurrentValue);

        public void Raise(T parameter) => Invoke(parameter);

        [Obsolete("A typed event is raised with a value of its own type.", true)]
        public void Raise(object value) => throw new NotSupportedException();

        public void AddListener(Action<T> method)
        {
            response.AddListener(method);
            RippleConnectionRegistry.RecordSubscribe(this, method);
        }

        public void RemoveListener(Action<T> method)
        {
            response.RemoveListener(method);
            RippleConnectionRegistry.RecordUnsubscribe(this, method);
        }

        protected override void ClearListeners()
        {
            base.ClearListeners();
            response.ClearListeners();
        }
    }

    public abstract class GameEvent : RippleStackTraceSO
    {
        [SerializeField, Tooltip("Drop the listeners added from code each time play mode starts. For projects that enter play mode without a domain reload.")]
        protected bool clearListenersOnPlaymode;

        private Action listeners;

        public virtual bool HasListeners => listeners != null;

        public void AddListener(Action method)
        {
            listeners += method;
            RippleConnectionRegistry.RecordSubscribe(this, method);
        }

        public void RemoveListener(Action method)
        {
            listeners -= method;
            RippleConnectionRegistry.RecordUnsubscribe(this, method);
        }

        public void Raise<T>(T value)
        {
            if (this is GameEvent<T> typed)
                typed.Invoke(value);
            else if (this is VoidEventSO plain)
                plain.Invoke();
            else
                throw new InvalidOperationException($"{name} ({GetType().Name}) cannot be raised with a {typeof(T).Name}.");
        }

        protected void NotifyListeners() => listeners?.Invoke();

        protected virtual void ClearListeners() => listeners = null;

        protected void OnEnable()
        {
            hideFlags = HideFlags.DontUnloadUnusedAsset;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
        }

#if UNITY_EDITOR
        private void OnDisable() => UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;

        private void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange state)
        {
            if (state != UnityEditor.PlayModeStateChange.ExitingEditMode) return;
            ClearInvokers();
            if (clearListenersOnPlaymode)
                ClearListeners();
        }
#endif
    }
}
