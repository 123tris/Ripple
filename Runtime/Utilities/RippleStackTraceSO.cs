using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Ripple
{
    public abstract class RippleStackTraceSO : ScriptableObject
    {
#if UNITY_EDITOR
        [SerializeField, TextArea, HideInInlineEditors]
        private string _developerNotes;

        [SerializeField, BoxGroup("Debug", order: 1), HideInInlineEditors, HorizontalGroup("Debug/Disable"), LabelWidth(100)]
        private bool disableLogging = false;
        
        [SerializeField, BoxGroup("Debug", order: 1), HideInInlineEditors, HorizontalGroup("Debug/Disable"), LabelWidth(125)]
        private bool disableStackTrace = false;

        [ShowInInspector, DisplayAsString, ShowIf("@invokeStackTraces.Count > 0"),
         LabelText("This Event is getting called by:"), BoxGroup("Debug", order: 1), HideInInlineEditors]
        protected List<string> invokeStackTraces = new();

#endif

        protected void LogInvoke<T>(T parameter)
        {
#if UNITY_EDITOR

            if (!disableStackTrace)
            {
                if (this is NumericalVariable<T>)
                    invokeStackTraces.Add(GetCaller(4));
                else
                    invokeStackTraces.Add(GetCaller(3));
            }
            
            if (disableLogging) return;
            Logger.Log(
                $"Called by: <color=red>{invokeStackTraces.LastOrDefault()}</color> \nWith value: <color=green>{parameter}</color>",
                this);
#endif
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private protected string GetCaller(int level)
        {
            System.Diagnostics.StackTrace stackTrace = new();

            Type declaringType = stackTrace.GetFrame(level).GetMethod().DeclaringType.BaseType;
            if (declaringType == typeof(System.Reflection.MethodInfo))
            {
                level = stackTrace.FrameCount - 1;
                // if (stackTrace.FrameCount >= 6 && stackTrace.GetFrame(6).GetMethod().DeclaringType ==
                //     typeof(UltEvents.PersistentCall))
                // {
                //     //being called by ultevent
                // }
            }

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