#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ripple
{
    public struct TraceFrame
    {
        public Guid TraceId;
        public int HopIndex;
        public int ParentHopIndex;
        public UnityEngine.Object Source;
        public string SourceName;
        public string PayloadString;
        public string CallerMember;
        public string CallerFile;
        public int CallerLine;
        public int FrameNumber;
        public double RealtimeSeconds;
    }

    public static class TraceStore
    {
        private const int MaxStoredTraces = 200;

        private static readonly Dictionary<Guid, List<TraceFrame>> _inProgress = new Dictionary<Guid, List<TraceFrame>>();
        private static readonly LinkedList<KeyValuePair<Guid, List<TraceFrame>>> _committed
            = new LinkedList<KeyValuePair<Guid, List<TraceFrame>>>();

        public static event Action<Guid, IReadOnlyList<TraceFrame>> OnTraceCommitted;

        public static IEnumerable<KeyValuePair<Guid, List<TraceFrame>>> CommittedTraces => _committed;

        public static IReadOnlyList<TraceFrame> GetTrace(Guid id)
        {
            foreach (var node in _committed)
                if (node.Key == id) return node.Value;
            return null;
        }

        public static void Clear()
        {
            _inProgress.Clear();
            _committed.Clear();
        }

        [UnityEditor.InitializeOnLoadMethod]
        private static void RegisterPlayModeHook()
        {
            UnityEditor.EditorApplication.playModeStateChanged += state =>
            {
                if (state == UnityEditor.PlayModeStateChange.ExitingEditMode) Clear();
            };
        }

        internal static void RecordFrame<T>(Guid traceId, int hopIndex, RippleStackTraceSO source, T payload,
            string callerMember, string callerFile, int callerLine, RippleTraceFrameHandle parent)
        {
            if (!_inProgress.TryGetValue(traceId, out var list))
            {
                list = new List<TraceFrame>();
                _inProgress[traceId] = list;
            }
            list.Add(new TraceFrame
            {
                TraceId = traceId,
                HopIndex = hopIndex,
                ParentHopIndex = parent?.HopIndex ?? -1,
                Source = source,
                SourceName = source != null ? source.name : "<destroyed>",
                PayloadString = payload?.ToString() ?? "null",
                CallerMember = callerMember,
                CallerFile = callerFile,
                CallerLine = callerLine,
                FrameNumber = Time.frameCount,
                RealtimeSeconds = Time.realtimeSinceStartupAsDouble,
            });
        }

        internal static void CommitTrace(Guid traceId)
        {
            if (!_inProgress.TryGetValue(traceId, out var list)) return;
            _inProgress.Remove(traceId);
            _committed.AddFirst(new KeyValuePair<Guid, List<TraceFrame>>(traceId, list));
            while (_committed.Count > MaxStoredTraces) _committed.RemoveLast();

            try { OnTraceCommitted?.Invoke(traceId, list); }
            catch (Exception e) { Debug.LogException(e); }
        }
    }
}
#endif
