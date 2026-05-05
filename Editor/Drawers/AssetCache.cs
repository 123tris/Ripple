using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Ripple.Editor
{
    public static class AssetCache
    {
        private static readonly Dictionary<Type, ScriptableObject[]> _cache = new();
        private static bool _hooked;

        public static ScriptableObject[] Find(Type type)
        {
            EnsureHook();
            if (_cache.TryGetValue(type, out var cached) && cached != null) return cached;
            var guids = AssetDatabase.FindAssets("t:" + type.Name);
            var result = guids
                .Select(g => AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(g), type))
                .OfType<ScriptableObject>()
                .Where(a => a != null && type.IsAssignableFrom(a.GetType()))
                .ToArray();
            _cache[type] = result;
            return result;
        }

        public static void Invalidate() => _cache.Clear();

        private static void EnsureHook()
        {
            if (_hooked) return;
            _hooked = true;
            CachePostprocessor.OnAssetsChanged += Invalidate;
        }

        private sealed class CachePostprocessor : AssetPostprocessor
        {
            public static event Action OnAssetsChanged;
            private static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
                => OnAssetsChanged?.Invoke();
        }
    }
}
