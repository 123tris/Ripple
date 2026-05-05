using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ripple
{
    public abstract class RippleStackTraceSO : ScriptableObject
    {
#if UNITY_EDITOR
        [SerializeField, TextArea] private string _developerNotes;

        [SerializeField] private bool recordHistory = false;
        [SerializeField] private bool recordStackTrace = false;
        [SerializeField] internal bool recordTrace = false;
        [SerializeField, Range(1, 1024)] private int historyDepth = 64;

        private readonly Queue<InvokeRecord> _history = new Queue<InvokeRecord>();

        public IReadOnlyCollection<InvokeRecord> History => _history;
        public bool RecordTrace => recordTrace;

        public void ClearDebug() => _history.Clear();
#endif

        protected void LogInvoke<T>(T payload, string callerMember, string callerFile, int callerLine)
        {
#if UNITY_EDITOR
            string callSite = $"{callerMember} ({System.IO.Path.GetFileName(callerFile)}:{callerLine})";

            string stack = recordStackTrace
                ? new System.Diagnostics.StackTrace(2, true).ToString()
                : null;

            if (recordHistory)
            {
                _history.Enqueue(new InvokeRecord
                {
                    timestampUtc = DateTime.UtcNow,
                    frame = Time.frameCount,
                    callSite = callSite,
                    payloadString = payload?.ToString() ?? "null",
                    stackTrace = stack,
                });
                while (_history.Count > historyDepth) _history.Dequeue();
            }

            RippleLogger.Log(this, callSite, payload, stack);
#endif
        }

#if UNITY_EDITOR
        public struct InvokeRecord
        {
            public DateTime timestampUtc;
            public int frame;
            public string callSite;
            public string payloadString;
            public string stackTrace;
        }
#endif
    }
}
