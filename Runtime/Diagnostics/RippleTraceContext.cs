using System;
using UnityEngine;

namespace Ripple
{
    public sealed class RippleTraceFrameHandle : IDisposable
    {
        internal Guid TraceId;
        internal int HopIndex;
        internal RippleTraceFrameHandle Parent;
        internal bool Active;
        internal bool IsRoot;

        private static readonly System.Collections.Generic.Stack<RippleTraceFrameHandle> _pool
            = new System.Collections.Generic.Stack<RippleTraceFrameHandle>();

        internal static RippleTraceFrameHandle Rent() =>
            _pool.Count > 0 ? _pool.Pop() : new RippleTraceFrameHandle();

        public void Dispose() => RippleTraceContext.PopFrame(this);

        internal void Reset()
        {
            TraceId = Guid.Empty;
            HopIndex = 0;
            Parent = null;
            Active = false;
            IsRoot = false;
        }

        internal void ReturnToPool()
        {
            Reset();
            _pool.Push(this);
        }
    }

    public static class RippleTraceContext
    {
        [ThreadStatic] private static RippleTraceFrameHandle _current;

        public const int MaxHops = 32;

        public static RippleTraceFrameHandle Current => _current;

        private sealed class NoopHandle : IDisposable
        {
            public static readonly NoopHandle Instance = new NoopHandle();
            public void Dispose() { }
        }

#if UNITY_EDITOR
        public static IDisposable PushFrame<T>(RippleStackTraceSO source, T payload,
            string callerMember, string callerFile, int callerLine)
        {
            if (source != null && !source.RecordTrace && _current == null)
                return NoopHandle.Instance;

            var parent = _current;
            int depth = 0;
            for (var p = parent; p != null; p = p.Parent) depth++;
            if (depth >= MaxHops)
            {
                Debug.LogWarning($"[Ripple] Trace depth exceeded {MaxHops} on '{source?.name}'. Aborting trace push.", source);
                return NoopHandle.Instance;
            }

            var handle = RippleTraceFrameHandle.Rent();
            handle.TraceId = parent?.TraceId ?? Guid.NewGuid();
            handle.HopIndex = (parent?.HopIndex ?? -1) + 1;
            handle.Parent = parent;
            handle.Active = true;
            handle.IsRoot = parent == null;

            TraceStore.RecordFrame(handle.TraceId, handle.HopIndex, source, payload,
                callerMember, callerFile, callerLine, parent);

            _current = handle;
            return handle;
        }

        internal static void PopFrame(RippleTraceFrameHandle handle)
        {
            if (handle == null || !handle.Active) return;
            if (_current != handle)
            {
                Debug.LogWarning("[Ripple] Trace frame mismatch on pop; restoring stack.");
                while (_current != null && _current != handle)
                {
                    var p = _current.Parent;
                    _current.ReturnToPool();
                    _current = p;
                }
                if (_current != handle) return;
            }

            bool wasRoot = handle.IsRoot;
            Guid id = handle.TraceId;
            _current = handle.Parent;
            handle.ReturnToPool();
            if (wasRoot) TraceStore.CommitTrace(id);
        }
#else
        public static IDisposable PushFrame<T>(RippleStackTraceSO source, T payload,
            string callerMember, string callerFile, int callerLine) => NoopHandle.Instance;

        internal static void PopFrame(RippleTraceFrameHandle handle) { }
#endif
    }
}
