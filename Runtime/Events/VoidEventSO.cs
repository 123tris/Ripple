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
                    invokeStackTraces.Clear();
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
            invokeStackTraces.Add(GetCaller(2));
            Logger.Log($"Called by: <color=red>{invokeStackTraces.Last()}</color>", this);
#endif
            gameEvent?.Invoke();
        }

        public bool HasListeners => gameEvent != null;

        public void AddListener(Action method) => gameEvent += method;

        public void RemoveListener(Action method) => gameEvent -= method;
    }
}