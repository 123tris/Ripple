using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace Ripple
{
    [Serializable]
    internal class TraceLogRecorder
    {
        [UnityEngine.SerializeField] private bool disableLogging;
        [UnityEngine.SerializeField] private bool disableStackTrace;
        [UnityEngine.SerializeField] private List<TraceSnapshot> callTraceHistory = new();

        internal bool IsLoggingDisabled => disableLogging;
        internal bool IsStackTraceDisabled => disableStackTrace;
        internal IReadOnlyList<TraceSnapshot> CallTraceHistory => callTraceHistory;

        internal void AddSnapshot(string caller, string fullStackTrace)
        {
            callTraceHistory.Add(new TraceSnapshot
            {
                time = DateTime.Now.ToString("HH:mm:ss.fff"),
                caller = string.IsNullOrWhiteSpace(caller) ? "Caller unknown" : caller,
                stack = string.IsNullOrWhiteSpace(fullStackTrace) ? "Stack trace disabled." : fullStackTrace
            });

            const int maxSnapshots = 25;
            if (callTraceHistory.Count > maxSnapshots)
                callTraceHistory.RemoveAt(0);
        }

        internal void ClearHistory()
        {
            callTraceHistory.Clear();
        }
    }

    [Serializable]
    internal class TraceSnapshot
    {
        [DisplayAsString, LabelWidth(70)]
        public string time;
        [DisplayAsString, LabelWidth(70)]
        public string caller;
        [MultiLineProperty(6), LabelWidth(70)]
        public string stack;
    }
}
