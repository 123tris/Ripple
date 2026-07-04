using System;
using System.Collections.Generic;
using System.Reflection;

namespace Ripple
{
    /// <summary>
    /// A runtime-captured subscriber for a single event/variable. <see cref="Target"/> is a weak
    /// reference so the registry never extends a subscriber's lifetime; <see cref="MethodName"/> /
    /// <see cref="DeclaringType"/> preserve identity even after the target has been collected.
    /// </summary>
    public readonly struct SubscriberInfo
    {
        public readonly WeakReference<object> Target; // null for static subscribers
        public readonly string MethodName;
        public readonly string DeclaringType;

        public SubscriberInfo(WeakReference<object> target, string methodName, string declaringType)
        {
            Target = target;
            MethodName = methodName;
            DeclaringType = declaringType;
        }
    }

    /// <summary>
    /// Editor-only registry of <em>dynamic</em> (runtime <c>+=</c>) subscribers, keyed by the
    /// event/variable asset they subscribe to. Populated through the <c>AddListener</c>/
    /// <c>RemoveListener</c> funnels on events and variables. Serialized (UltEvent persistent)
    /// listeners are not tracked here — they are enumerated separately by editor tooling.
    ///
    /// All state and behaviour are compiled only under UNITY_EDITOR; in player builds every method
    /// is a no-op so the runtime call sites cost nothing.
    /// </summary>
    public static class RippleConnectionRegistry
    {
#if UNITY_EDITOR
        private struct Entry
        {
            public WeakReference<object> Target; // null => static subscriber
            public MethodInfo Method;            // stored instead of the Delegate so we never pin the target
        }

        private static readonly Dictionary<RippleStackTraceSO, List<Entry>> Map = new();

        static RippleConnectionRegistry()
        {
            UnityEditor.EditorApplication.playModeStateChanged += state =>
            {
                if (state == UnityEditor.PlayModeStateChange.ExitingEditMode)
                    Map.Clear();
            };
        }

        private static bool SameSubscription(Entry e, Delegate method)
        {
            if (!Equals(e.Method, method.Method)) return false;

            object target = method.Target;
            if (e.Target == null) return target == null; // both static
            return e.Target.TryGetTarget(out var t) && ReferenceEquals(t, target);
        }
#endif

        public static void RecordSubscribe(RippleStackTraceSO source, Delegate method)
        {
#if UNITY_EDITOR
            if (source == null || method == null) return;
            if (!Map.TryGetValue(source, out var list))
            {
                list = new List<Entry>();
                Map[source] = list;
            }

            if (list.Exists(e => SameSubscription(e, method))) return; // avoid duplicates across enable cycles

            object target = method.Target;
            list.Add(new Entry
            {
                Target = target != null ? new WeakReference<object>(target) : null,
                Method = method.Method,
            });
#endif
        }

        public static void RecordUnsubscribe(RippleStackTraceSO source, Delegate method)
        {
#if UNITY_EDITOR
            if (source == null || method == null) return;
            if (Map.TryGetValue(source, out var list))
                list.RemoveAll(e => SameSubscription(e, method));
#endif
        }

        /// <summary>Live subscribers for <paramref name="source"/>. Prunes entries whose target was collected.</summary>
        public static IReadOnlyList<SubscriberInfo> GetSubscribers(RippleStackTraceSO source)
        {
            var result = new List<SubscriberInfo>();
#if UNITY_EDITOR
            if (source != null && Map.TryGetValue(source, out var list))
            {
                foreach (var e in list)
                {
                    result.Add(new SubscriberInfo(
                        e.Target,
                        e.Method?.Name,
                        e.Method?.DeclaringType?.FullName));
                }
            }
#endif
            return result;
        }

        public static void Clear()
        {
#if UNITY_EDITOR
            Map.Clear();
#endif
        }
    }
}
