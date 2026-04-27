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
        [FormerlySerializedAs("gameEvent")]
        [SerializeField, PropertySpace(SpaceAfter = 10)]
        private UltEvent<T> response;

        protected void OnEnable()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged += state =>
            {
                if (state == UnityEditor.PlayModeStateChange.ExitingEditMode)
                {
                    stackTrace.Clear();
                    if (clearListenersOnPlaymode)
                        response.Clear();
                }
            };
#endif
            hideFlags = HideFlags.DontUnloadUnusedAsset;
        }

        [Button, BoxGroup("Debug")]
        public virtual void Invoke(T parameter, UnityEngine.Object context)
        {
            LogInvoke(parameter, context);
            if (response == null) return;

            if (!response.HasCalls)
                Debug.LogWarning($"Calling event \"{name}\" but no listeners were found", this);

            response.Invoke(parameter);
        }

        public virtual void Invoke(T parameter)
        {
            Invoke(parameter, null);
        }

        public void Invoke(VariableSO<T> parameter, UnityEngine.Object context)
        {
            Invoke(parameter.CurrentValue, context);
        }

        public void Invoke(VariableSO<T> parameter)
        {
            Invoke(parameter.CurrentValue, null);
        }

        public bool HasListeners => response != null;

        public void AddListener(Action<T> method) => response += method;

        public void RemoveListener(Action<T> method) => response -= method;

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