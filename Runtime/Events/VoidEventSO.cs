using System;
using System.Linq;
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
            UnityEditor.EditorApplication.playModeStateChanged += state =>
            {
                if (state == UnityEditor.PlayModeStateChange.ExitingEditMode)
                {
                    stackTrace.Clear();
                    if (clearListenersOnPlaymode)
                        gameEvent.Clear();
                }
            };
#endif
        }

        [Button]
        public void Invoke()
        {
#if UNITY_EDITOR
            StacktraceItem item = GetStracktraceItem(STACK_TRACE_DEPTH);
            item.name = GetCaller(2);
            stackTrace.Add(item);
            Logger.Log($"Called by: <color=red>{stackTrace.Last()}</color>", this);
#endif
            gameEvent?.Invoke();
        }

        public bool HasListeners => gameEvent != null;

        public void AddListener(Action method) => gameEvent += method;

        public void RemoveListener(Action method) => gameEvent -= method;

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