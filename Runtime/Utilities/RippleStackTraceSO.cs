using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

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

        [SerializeField, BoxGroup("Debug", order: 1), HideInInlineEditors, LabelText("Scan prefabs too")]
        private bool _scanPrefabs = true;

        [ShowInInspector, TableList(IsReadOnly = true, AlwaysExpanded = true), ShowIf("@Invokers.Count > 0"),
         LabelText("Invoked by"), BoxGroup("Debug", order: 1), HideInInlineEditors]
        protected readonly List<InvokeRecord> Invokers = new();

        [ShowInInspector, TableList(IsReadOnly = true, AlwaysExpanded = true), ShowIf("@_listeners.Count > 0"),
         LabelText("Listeners"), BoxGroup("Debug", order: 1), HideInInlineEditors]
        private readonly List<RippleConnections.ListenerRow> _listeners = new();

        [Button("Scan Listeners"), BoxGroup("Debug", order: 1), HideInInlineEditors]
        private void ScanListeners() => RippleConnections.PopulateListeners(this, _listeners, _scanPrefabs);

        public sealed class InvokeRecord
        {
            [TableColumnWidth(190, false)] public string Caller;
            [TableColumnWidth(55, false)] public int Count;
            public string LastValue;
            [HideInTables, HideInInspector] public UnityEngine.Object Sender;
            [HideInTables, HideInInspector] public double LastTimeStamp;
            [HideInTables, HideInInspector] public string Trace;

            [Button("Ping"), ShowIf("@Sender != null"), TableColumnWidth(60, false)]
            private void Ping() => RippleConnections.Ping(Sender);

            [Button("Trace"), ShowIf("@Trace != null"), TableColumnWidth(60, false)]
            private void LogTrace() => Debug.Log($"{Caller}\n{Trace}", Sender);
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
            rec.LastTimeStamp = UnityEditor.EditorApplication.timeSinceStartup;

            if (disableLogging) return;
            string message = valueText != null
                ? $"Called by: <color=red>{caller}</color> \nWith value: <color=green>{valueText}</color>"
                : $"Called by: <color=red>{caller}</color>";
            Logger.Log(message, this);
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
            return ns != null && (ns.StartsWith("UnityEngine.Events") || ns.StartsWith("System.Reflection"));
        }

        protected void ClearInvokers() => Invokers.Clear();
#endif
    }
}
