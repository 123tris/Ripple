using System;
using Sirenix.OdinInspector;
using UltEvents;
using UnityEngine;

namespace Ripple
{
    [InlineEditor]
    public class GameEvent<T> : ScriptableObject
    {
#if UNITY_EDITOR
        [SerializeField, TextArea(0, 5, order = 5), HideInInlineEditors]
        private string _developerNotes;

#endif
        [SerializeField] private bool showDebug;

        [SerializeField] private UltEvent<T> gameEvent;

        protected void OnEnable()
        {
            hideFlags = HideFlags.DontUnloadUnusedAsset;
        }

        [Button]
        public void Invoke(T parameter)
        {
// #if UNITY_EDITOR
//             invokeStackTraces.Add(GetCaller());
// #endif
            if (gameEvent == null) return;

            if (!gameEvent.HasCalls)
                Debug.LogWarning($"Calling event \"{name}\" but no listeners were found", this);
            
            gameEvent.Invoke(parameter);
            if (showDebug) //TODO: Should be turned into custom logger
                Debug.Log($"Fired event: {name}, with parameter: {parameter}");
        }

        public void Invoke(VariableSO<T> parameter)
        {
            Invoke(parameter.CurrentValue);
        }

        public bool HasListeners => gameEvent != null;

        public void AddListener(Action<T> method) => gameEvent += method;

        public void RemoveListener(Action<T> method) => gameEvent -= method;
    }
}