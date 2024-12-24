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
        [SerializeField]
        private UltEvent gameEvent;

        protected void OnEnable()
        {
            hideFlags = HideFlags.DontUnloadUnusedAsset;
        }

        [Button]
        public void Invoke()
        {
            invokeStackTraces.Add(GetCaller());
            Logger.Log($"Called by: <color=red>{invokeStackTraces.Last()}</color>", this);
            gameEvent?.Invoke();
        }

        public bool HasListeners => gameEvent != null;

        public void AddListener(Action method) => gameEvent += method;

        public void RemoveListener(Action method) => gameEvent -= method;
    }
}
