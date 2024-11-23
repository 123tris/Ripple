using System;
using Sirenix.OdinInspector;
using UltEvents;
using UnityEngine;

namespace Ripple
{
    public class GameEvent<T> : ScriptableObject
    {
#if UNITY_EDITOR
        [SerializeField, TextArea] private string _developerNotes;
#endif

        [SerializeField]
        private UltEvent<T> gameEvent;

        protected void OnEnable()
        {
            hideFlags = HideFlags.DontUnloadUnusedAsset;
        }

        [Button]
        public void Invoke(T parameter)
        {
            gameEvent?.Invoke(parameter);
        }

        public bool HasListeners => gameEvent != null;

        public void AddListener(Action<T> method) => gameEvent += method;

        public void RemoveListener(Action<T> method) => gameEvent -= method;
    }
}