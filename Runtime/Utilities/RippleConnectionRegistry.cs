using System;
using System.Collections.Generic;
using System.Reflection;

namespace Ripple
{
    public readonly struct SubscriberInfo
    {
        public readonly WeakReference<object> Target;
        public readonly string MethodName;
        public readonly string DeclaringType;

        public SubscriberInfo(WeakReference<object> target, string methodName, string declaringType)
        {
            Target = target;
            MethodName = methodName;
            DeclaringType = declaringType;
        }
    }

    public static class RippleConnectionRegistry
    {
#if UNITY_EDITOR
        private struct Entry
        {
            public WeakReference<object> Target;
            public MethodInfo Method;
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
            if (e.Target == null) return target == null;
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

            if (list.Exists(e => SameSubscription(e, method))) return;

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
