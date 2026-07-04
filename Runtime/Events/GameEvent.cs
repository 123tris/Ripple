using System;
using Sirenix.OdinInspector;
using UltEvents;
using UnityEngine;
using UnityEngine.Serialization;

namespace Ripple
{
    [InlineEditor]
    public class GameEvent<T> : GameEvent
    {
        [FormerlySerializedAs("gameEvent")]
        [SerializeField]
        private UltEvent<T> response;

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
                response.Clear();
        }
#endif

        [Button, BoxGroup("Debug")]
        public virtual void Invoke(T parameter, UnityEngine.Object sender = null)
        {
            LogInvoke(parameter, sender);
            if (response == null) return;

            if (!response.HasCalls)
                Debug.LogWarning($"Calling event \"{name}\" but no listeners were found", this);

            response.Invoke(parameter);
        }

        public void Invoke(VariableSO<T> parameter) => Invoke(parameter.CurrentValue);

        public bool HasListeners => response != null && response.HasCalls;

        public void AddListener(Action<T> method)
        {
            response += method;
            RippleConnectionRegistry.RecordSubscribe(this, method);
        }

        public void RemoveListener(Action<T> method)
        {
            response -= method;
            RippleConnectionRegistry.RecordUnsubscribe(this, method);
        }

        public override void AddGenericListener(Delegate method)
        {
            response.AddPersistentCall(method);
        }

        public override void RemoveGenericListener(Delegate method)
        {
            response.RemovePersistentCall(method);
        }
    }

    public abstract class GameEvent : RippleStackTraceSO
    {
        public abstract void AddGenericListener(Delegate method);

        public abstract void RemoveGenericListener(Delegate method);
        [SerializeField, BoxGroup("Debug")] protected bool clearListenersOnPlaymode;
    }
}
