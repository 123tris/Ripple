using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace Ripple
{
    public abstract class RippleStackTraceSO : ScriptableObject
    {
#if UNITY_EDITOR
        [SerializeField] private string _developerNotes;

        [SerializeField, Tooltip("Keep this asset out of the Ripple Debugger log.")]
        private bool disableLogging;

        [SerializeField, Tooltip("Skip capturing the call stack. Callers are then named only when they pass themselves as the sender.")]
        private bool disableStackTrace;

        internal readonly List<InvokeRecord> Invokers = new();

        public sealed class InvokeRecord
        {
            public string Caller;
            public int Count;
            public string LastValue;
            public string LastTime;
            public UnityEngine.Object Sender;
            public string Trace;
        }
#endif

        internal virtual UnityEventBase SerializedResponse => null;

        protected void LogInvoke<T>(T parameter, UnityEngine.Object sender = null)
        {
#if UNITY_EDITOR
            RecordInvoke(ResolveCaller(sender), sender, parameter != null ? parameter.ToString() : "null");
#endif
        }

#if UNITY_EDITOR
        private protected void RecordInvoke(string caller, UnityEngine.Object sender, string valueText)
        {
            InvokeRecord rec = Invokers.Find(r => r.Caller == caller && r.Sender == sender);
            if (rec == null)
            {
                rec = new InvokeRecord { Caller = caller, Sender = sender, Trace = disableStackTrace ? null : DescribeStack() };
                Invokers.Add(rec);
            }

            rec.Count++;
            rec.LastValue = valueText;
            rec.LastTime = DateTime.Now.ToString("HH:mm:ss.fff");

            if (!disableLogging)
                Logger.Log(this, caller, valueText, rec.LastTime);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private protected string ResolveCaller(UnityEngine.Object sender)
        {
            if (sender != null) return sender.name;
            if (disableStackTrace) return "(stack trace disabled)";

            var stackTrace = new System.Diagnostics.StackTrace();
            for (int i = 1; i < stackTrace.FrameCount; i++)
            {
                var method = stackTrace.GetFrame(i)?.GetMethod();
                if (method == null || IsPlumbing(method.DeclaringType)) continue;
                return (method.DeclaringType?.FullName ?? "(global)") + "->" + method.Name;
            }

            return "(unknown)";
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static string DescribeStack()
        {
            var text = new StringBuilder();
            string project = Directory.GetCurrentDirectory();
            foreach (System.Diagnostics.StackFrame frame in new System.Diagnostics.StackTrace(true).GetFrames())
            {
                var method = frame.GetMethod();
                if (method == null || text.Length == 0 && IsPlumbing(method.DeclaringType)) continue;
                text.Append(method.DeclaringType?.FullName ?? "(global)").Append(':').Append(method.Name).Append(" ()");
                string file = frame.GetFileName();
                if (file != null)
                    text.Append(" (at ").Append(Path.GetRelativePath(project, file).Replace('\\', '/')).Append(':').Append(frame.GetFileLineNumber()).Append(')');
                text.Append('\n');
            }

            return text.ToString();
        }

        private static bool IsPlumbing(Type type)
        {
            if (type == null) return false;
            if (typeof(RippleStackTraceSO).IsAssignableFrom(type)) return true;
            if (type.IsGenericType) type = type.GetGenericTypeDefinition();
            if (type == typeof(Signal) || type == typeof(Signal<>)) return true;
            string ns = type.Namespace;
            return ns != null && (ns.StartsWith("UnityEngine.Events") || ns.StartsWith("System.Reflection") || ns.StartsWith("Unity.Properties"));
        }

        protected void ClearInvokers() => Invokers.Clear();
#endif
    }
}
