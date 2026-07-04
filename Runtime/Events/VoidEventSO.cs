using System;
using Sirenix.OdinInspector;
using UltEvents;
using UnityEngine;

namespace Ripple
{
    [RippleData]
    [CreateAssetMenu(menuName = "Events/Void")]
    public class VoidEventSO : GameEvent
    {
        [SerializeField] private UltEvent gameEvent;

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
                gameEvent.Clear();
        }
#endif

        [Button]
        public void Invoke(UnityEngine.Object sender = null)
        {
#if UNITY_EDITOR
            RecordInvoke(ResolveVoidCaller(sender), sender, null);
#endif
            gameEvent?.Invoke();
        }

        public bool HasListeners => gameEvent != null && gameEvent.HasCalls;

        public void AddListener(Action method)
        {
            gameEvent += method;
            RippleConnectionRegistry.RecordSubscribe(this, method);
        }

        public void RemoveListener(Action method)
        {
            gameEvent -= method;
            RippleConnectionRegistry.RecordUnsubscribe(this, method);
        }

        public override void AddGenericListener(Delegate method)
        {
            gameEvent.AddPersistentCall(method);
        }

        public override void RemoveGenericListener(Delegate method)
        {
            gameEvent.RemovePersistentCall(method);
        }
    }
}
