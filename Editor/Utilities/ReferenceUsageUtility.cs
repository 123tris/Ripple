using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Ripple.Editor
{
    internal static class ReferenceUsageUtility
    {
        private const double CacheTtlSeconds = 2.0;

        private static readonly Dictionary<UnityEngine.Object, (ReferenceUsageSummary summary, double builtAt)> _cache = new();

        static ReferenceUsageUtility()
        {
            EditorApplication.projectChanged += InvalidateAll;
        }

        internal static void InvalidateCache(UnityEngine.Object target)
        {
            _cache.Remove(target);
        }

        private static void InvalidateAll()
        {
            _cache.Clear();
        }

        internal static ReferenceUsageSummary BuildSummary(UnityEngine.Object target)
        {
            if (target == null)
                return new ReferenceUsageSummary();

            double now = EditorApplication.timeSinceStartup;
            if (_cache.TryGetValue(target, out var cached) && (now - cached.builtAt) < CacheTtlSeconds)
                return cached.summary;

            var summary = ScanReferences(target);
            _cache[target] = (summary, now);
            return summary;
        }

        private static ReferenceUsageSummary ScanReferences(UnityEngine.Object target)
        {
            var summary = new ReferenceUsageSummary();

            var allObjects = Resources.FindObjectsOfTypeAll<UnityEngine.Object>();
            foreach (var owner in allObjects)
            {
                if (owner == null || owner == target)
                    continue;

                if (owner is not MonoBehaviour && owner is not ScriptableObject)
                    continue;

                var serializedObject = new SerializedObject(owner);
                var iterator = serializedObject.GetIterator();
                if (!iterator.NextVisible(true))
                    continue;

                while (iterator.NextVisible(false))
                {
                    if (iterator.propertyType != SerializedPropertyType.ObjectReference)
                        continue;
                    if (iterator.objectReferenceValue != target)
                        continue;

                    bool isListenReference = IsListenReference(owner, iterator.propertyPath);
                    if (isListenReference)
                        summary.ListenReferences++;
                    else
                        summary.InvokeReferences++;

                    if (summary.ExampleReferences.Count < 20)
                    {
                        string usageTag = isListenReference ? "LISTEN" : "INVOKE";
                        summary.ExampleReferences.Add($"{usageTag}: {owner.name}/{owner.GetType().Name}.{iterator.propertyPath}");
                    }
                }
            }

            return summary;
        }

        private static bool IsListenReference(UnityEngine.Object owner, string propertyPath)
        {
            var field = owner.GetType().GetField(propertyPath, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null && field.GetCustomAttribute<ListenerReferenceAttribute>() != null)
                return true;

            string typeName = owner.GetType().Name;
            if (typeName.StartsWith("EventListener", StringComparison.Ordinal) ||
                typeName.StartsWith("VariableListener", StringComparison.Ordinal))
                return true;

            return propertyPath.IndexOf("onValueChanged", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   propertyPath.IndexOf("response", StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }

    public class ReferenceUsageSummary
    {
        internal int InvokeReferences { get; set; }
        internal int ListenReferences { get; set; }
        internal List<string> ExampleReferences { get; } = new();

        internal ReferenceUsageMode Mode
        {
            get
            {
                if (InvokeReferences > 0 && ListenReferences > 0)
                    return ReferenceUsageMode.InvokeAndListen;
                if (InvokeReferences > 0)
                    return ReferenceUsageMode.InvokeOnly;
                if (ListenReferences > 0)
                    return ReferenceUsageMode.ListenOnly;
                return ReferenceUsageMode.Unused;
            }
        }
    }

    public enum ReferenceUsageMode
    {
        Unused,
        InvokeOnly,
        ListenOnly,
        InvokeAndListen
    }
}
