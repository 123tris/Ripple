using System;
using System.Linq;
using Sirenix.OdinInspector;
using UltEvents;
using UnityEngine;
using UnityEngine.Serialization;

namespace Ripple
{
    [InlineEditor]
    public class GameEvent<T> : GameEvent
    {
        [FormerlySerializedAs("gameEvent")] [SerializeField]
        private UltEvent<T> response;

        protected void OnEnable()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged += state =>
            {
                if (state == UnityEditor.PlayModeStateChange.ExitingEditMode)
                    invokeStackTraces.Clear();
            };
#endif
            hideFlags = HideFlags.DontUnloadUnusedAsset;
        }

        [Button]
        public void Invoke(T parameter)
        {
            LogInvoke(parameter);
            if (response == null) return;

            if (!response.HasCalls)
                Debug.LogWarning($"Calling event \"{name}\" but no listeners were found", this);

            response.Invoke(parameter);
        }

        public void Invoke(VariableSO<T> parameter)
        {
            LogInvoke(parameter.CurrentValue);
            Invoke(parameter.CurrentValue);
        }

        public bool HasListeners => response != null;

        public void AddListener(Action<T> method) => response += method;

        public void RemoveListener(Action<T> method) => response -= method;
    }

    public abstract class GameEvent : RippleStackTraceSO { }
}