using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ripple
{
    public static class RippleLogger
    {
        public struct Entry
        {
            public DateTime timestampUtc;
            public int frame;
            public UnityEngine.Object source;
            public string sourceName;
            public string callSite;
            public string payloadString;
            public string stackTrace;
            public Guid traceId;
            public int hopIndex;
        }

        private static readonly List<Entry> _entries = new List<Entry>(capacity: 1024);
        private const int MaxEntries = 4096;

        public static event Action<Entry> OnEntryAdded;

        public static IReadOnlyList<Entry> Entries => _entries;

        public static void Clear()
        {
            _entries.Clear();
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        private static void RegisterPlayModeHook()
        {
            UnityEditor.EditorApplication.playModeStateChanged += state =>
            {
                if (state == UnityEditor.PlayModeStateChange.ExitingEditMode) Clear();
            };
        }
#endif

        public static void Log<T>(UnityEngine.Object source, string callSite, T payload, string stackTrace)
        {
#if UNITY_EDITOR
            var traceFrame = RippleTraceContext.Current;
            var entry = new Entry
            {
                timestampUtc = DateTime.UtcNow,
                frame = Time.frameCount,
                source = source,
                sourceName = source != null ? source.name : "<destroyed>",
                callSite = callSite,
                payloadString = payload?.ToString() ?? "null",
                stackTrace = stackTrace,
                traceId = traceFrame?.TraceId ?? Guid.Empty,
                hopIndex = traceFrame?.HopIndex ?? 0,
            };
            _entries.Add(entry);
            if (_entries.Count > MaxEntries) _entries.RemoveAt(0);
            try { OnEntryAdded?.Invoke(entry); } catch (Exception e) { Debug.LogException(e); }
#endif
        }
    }
}
