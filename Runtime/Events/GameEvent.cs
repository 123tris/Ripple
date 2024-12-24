using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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

            // Logger.Log($"Fired event: {name}, with parameter: {parameter}", this);
        }

        private void LogInvoke(T parameter)
        {
#if UNITY_EDITOR
            invokeStackTraces.Add(GetCaller(3));
            Logger.Log($"Called by: <color=red>{invokeStackTraces.Last()}</color> \nWith value: <color=green>{parameter}</color>", this);
#endif
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

    public abstract class GameEvent : ScriptableObject
    {
#if UNITY_EDITOR
        [SerializeField, TextArea] private string _developerNotes;

        [ShowInInspector, DisplayAsString, ShowIf("@invokeStackTraces.Count > 0"),
         LabelText("This Event is getting called by:")]
        protected List<string> invokeStackTraces = new();
#endif

        [MethodImpl(MethodImplOptions.NoInlining)]
        protected string GetCaller(int level = 2)
        {
            System.Diagnostics.StackTrace stackTrace = new();

            Type declaringType = stackTrace.GetFrame(level).GetMethod().DeclaringType.BaseType;
            if (declaringType == typeof(System.Reflection.MethodInfo))
                level = stackTrace.FrameCount - 1;

            var m = stackTrace.GetFrame(level).GetMethod();

            // .Name is the name only, .FullName includes the namespace
            var className = m.DeclaringType.FullName;

            //the method/function name you are looking for.
            var methodName = m.Name;

            //returns a composite of the namespace, class and method name.
            return className + "->" + methodName;
        }
    }
}