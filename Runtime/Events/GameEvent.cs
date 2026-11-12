using System;
using UnityEngine;
using UnityEngine.Events;

namespace Ripple
{
    public class GameEvent<T> : GameEvent
    {
        [SerializeField] private Signal<T> response = new();

        public override bool HasListeners => response.HasListeners;

        internal override UnityEventBase SerializedResponse => response.Response;

        public virtual void Invoke(T parameter, UnityEngine.Object sender = null)
        {
            LogInvoke(parameter, sender);
            response.Invoke(parameter);
        }

        public void Invoke(VariableSO<T> parameter) => Invoke(parameter.CurrentValue);

        public void Raise(T parameter) => Invoke(parameter);

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

        protected override void ClearListeners() => response.ClearListeners();
    }

    public abstract class GameEvent : RippleStackTraceSO
    {
        [SerializeField, Tooltip("Drop the listeners added from code each time play mode starts. For projects that enter play mode without a domain reload.")]
        protected bool clearListenersOnPlaymode;

        public abstract bool HasListeners { get; }

        protected abstract void ClearListeners();

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
