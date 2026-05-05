using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Ripple.Editor
{
    public static class ReferenceUsageIndex
    {
        private static Dictionary<string, List<string>> _usagesByGuid;
        private static bool _building;

        public static IReadOnlyList<string> FindUsages(ScriptableObject so)
        {
            if (so == null) return System.Array.Empty<string>();
            EnsureBuilt();
            var path = AssetDatabase.GetAssetPath(so);
            var guid = AssetDatabase.AssetPathToGUID(path);
            return _usagesByGuid != null && _usagesByGuid.TryGetValue(guid, out var list)
                ? (IReadOnlyList<string>)list
                : System.Array.Empty<string>();
        }

        public static void Rebuild() { _usagesByGuid = null; EnsureBuilt(); }

        private static void EnsureBuilt()
        {
            if (_usagesByGuid != null || _building) return;
            _building = true;
            try
            {
                var map = new Dictionary<string, List<string>>();
                var allAssets = AssetDatabase.FindAssets("t:GameObject t:ScriptableObject t:SceneAsset");
                foreach (var assetGuid in allAssets)
                {
                    var path = AssetDatabase.GUIDToAssetPath(assetGuid);
                    var deps = AssetDatabase.GetDependencies(path, false);
                    foreach (var dep in deps)
                    {
                        var depGuid = AssetDatabase.AssetPathToGUID(dep);
                        if (!map.TryGetValue(depGuid, out var list))
                        {
                            list = new List<string>();
                            map[depGuid] = list;
                        }
                        if (!list.Contains(path)) list.Add(path);
                    }
                }
                _usagesByGuid = map;
            }
            finally { _building = false; }
        }

        private sealed class IndexPostprocessor : AssetPostprocessor
        {
            private static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
            {
                _usagesByGuid = null;
            }
        }

        [MenuItem("CONTEXT/RippleStackTraceSO/Find References")]
        private static void FindReferencesMenu(MenuCommand cmd)
        {
            var so = cmd.context as ScriptableObject;
            if (so == null) return;
            var usages = FindUsages(so);
            if (usages.Count == 0) Debug.Log($"[Ripple] No usages found for '{so.name}'.", so);
            else
            {
                Debug.Log($"[Ripple] {usages.Count} usage(s) of '{so.name}':\n  " + string.Join("\n  ", usages), so);
            }
        }
    }
}
