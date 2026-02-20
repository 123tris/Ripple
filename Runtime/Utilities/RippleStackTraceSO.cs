using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace Ripple
{
    public abstract class RippleStackTraceSO : ScriptableObject
    {
#if UNITY_EDITOR
        [SerializeField, TextArea, HideInInlineEditors]
        private string _developerNotes;

        [BoxGroup("Debug", order: 1, CenterLabel = true, HideWhenChildrenAreInvisible = true)]
        [SerializeField, HideInInlineEditors, PropertySpace(0,5)]
        private bool disableLogging = false;

        [ShowInInspector, BoxGroup("Debug", order: 1), HideInInlineEditors, PropertySpace(0, 5)]
        [HideReferenceObjectPicker, ListDrawerSettings(DraggableItems = false,HideAddButton = true, HideRemoveButton = true)]
        protected List<StacktraceItem> stackTrace = new();
#endif
        private protected const int STACK_TRACE_DEPTH = 12;

        protected void LogInvoke<T>(T parameter)
        {
#if UNITY_EDITOR
            StacktraceItem item = GetStracktraceItem(STACK_TRACE_DEPTH);

            if (this is NumericalVariable<T>)
                item.name = GetCaller(4);
            else
                item.name = GetCaller(3);

            stackTrace.Add(item);
            if (disableLogging) return;
            Logger.Log(
                $"Called by: <color=red>{stackTrace.Last().name}</color> \nWith value: <color=green>{parameter}</color>",
                this);
#endif
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private protected string GetCaller(int level)
        {
            System.Diagnostics.StackTrace stackTrace = new();

            Type declaringType = stackTrace.GetFrame(level).GetMethod().DeclaringType.BaseType;
            if (declaringType == typeof(MethodInfo))
            {
                level = stackTrace.FrameCount - 1;
                // if (stackTrace.FrameCount >= 6 && stackTrace.GetFrame(6).GetMethod().DeclaringType ==
                //     typeof(UltEvents.PersistentCall))
                // {
                //     //being called by ultevent
                // }
            }

            var m = stackTrace.GetFrame(level).GetMethod();

            var className = m.DeclaringType.FullName;

            var methodName = m.Name;

            return className + "->" + methodName;
        }

        private protected StacktraceItem GetStracktraceItem(int stackTraceDepth, int skipFrames = 2)
        {
            var stackTrace = new System.Diagnostics.StackTrace(true);
            var frames = stackTrace.GetFrames();

            StacktraceItem output = new();
            var sb = new StringBuilder();

            for (int i = 0; i < stackTraceDepth && i < frames.Length; i++)
            {
                if (i < skipFrames)
                    continue;

                System.Diagnostics.StackFrame frame = frames[i];

                var method = frame.GetMethod();
                var m = frame.GetMethod();
                var className = m.DeclaringType.FullName;
                var methodName = m.Name;
                var file = frame.GetFileName();
                var line = frame.GetFileLineNumber();

                sb.AppendLine(
                    $"• <b>{method.DeclaringType?.Name}.{method.Name}</b>()" +
                    (file != null ? $"  ({System.IO.Path.GetFileName(file)}:{line})" : "")
                );
            }

            output.strackTraceText = sb.ToString();

            return output;
        }

        [Serializable]
        public class StacktraceItem
        {
            [HideInInspector]
            public string name;

            [HideLabel, FoldoutGroup("@name"), DisplayAsString(EnableRichText = true, Overflow = false)]
            public string strackTraceText;
        }
    }
}