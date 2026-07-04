using System.Collections.Generic;
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

        [SerializeField, BoxGroup("Debug", order: 1), HideInInlineEditors, LabelText("Scan prefabs too")]
        private bool _scanPrefabs = true;

        // Who invokes this event / changes this variable, aggregated per caller (Play mode).
        [ShowInInspector, TableList(IsReadOnly = true, AlwaysExpanded = true), ShowIf("@Invokers.Count > 0"),
         LabelText("Invoked by"), BoxGroup("Debug", order: 1), HideInInlineEditors]
        protected readonly List<InvokeRecord> Invokers = new();

        // Who listens to this event / variable. Filled on demand by the Scan button below.
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
            [HideInTables, HideInInspector] public Object Sender;
            [HideInTables, HideInInspector] public double LastTimeStamp;

            [Button("Ping"), ShowIf("@Sender != null"), TableColumnWidth(60, false)]
            private void Ping() => RippleConnections.Ping(Sender);
        }
#endif

        /// <summary>
        /// Records an invocation of this event / variable change. Editor-only; a no-op in player builds
        /// so call sites cost nothing. Pass <paramref name="sender"/> to attribute the invoke to a
        /// specific object; when null, the calling type/method is resolved from the stack trace.
        /// </summary>
        protected void LogInvoke<T>(T parameter, UnityEngine.Object sender = null)
        {
#if UNITY_EDITOR
            string caller;
            if (sender != null)
                caller = sender.name;
            else if (disableStackTrace)
                caller = "(stack trace disabled)";
            else
                // Frame depth from the public Invoke/SetCurrentValue down to GetCaller:
                //   Event / plain Variable: Invoke -> LogInvoke -> GetCaller .......... 3
                //   NumericalVariable adds its SetCurrentValue override frame .......... 4
                caller = GetCaller(this is NumericalVariable<T> ? 4 : 3);

            RecordInvoke(caller, sender, parameter != null ? parameter.ToString() : "null");
#endif
        }

#if UNITY_EDITOR
        /// <summary>
        /// Aggregation + logging shared by the value and void invoke paths. Does NOT walk the stack,
        /// so it can be called from anywhere without shifting <see cref="GetCaller"/> frame depths.
        /// </summary>
        private protected void RecordInvoke(string caller, UnityEngine.Object sender, string valueText)
        {
            InvokeRecord rec = Invokers.Find(r => r.Caller == caller && r.Sender == sender);
            if (rec == null)
            {
                rec = new InvokeRecord { Caller = caller, Sender = sender };
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

        /// <summary>Resolves the caller for a parameterless (void) invoke, honouring the sender override.</summary>
        private protected string ResolveVoidCaller(UnityEngine.Object sender)
        {
            if (sender != null) return sender.name;
            if (disableStackTrace) return "(stack trace disabled)";
            // VoidEventSO.Invoke -> ResolveVoidCaller -> GetCaller .......... 3
            return GetCaller(3);
        }

        protected void ClearInvokers() => Invokers.Clear();
#endif

        [MethodImpl(MethodImplOptions.NoInlining)]
        private protected string GetCaller(int level)
        {
            var stackTrace = new System.Diagnostics.StackTrace();
            if (level >= stackTrace.FrameCount) level = stackTrace.FrameCount - 1;
            if (level < 0) return "(unknown)";

            var method = stackTrace.GetFrame(level)?.GetMethod();
            if (method == null) return "(unknown)";

            // If the resolved frame is a reflection trampoline (Odin button / UltEvents persistent call),
            // fall back to the outermost frame.
            if (method.DeclaringType?.BaseType == typeof(System.Reflection.MethodInfo))
            {
                method = stackTrace.GetFrame(stackTrace.FrameCount - 1)?.GetMethod();
                if (method == null) return "(unknown)";
            }

            string className = method.DeclaringType?.FullName ?? "(global)";
            return className + "->" + method.Name;
        }
    }
}
