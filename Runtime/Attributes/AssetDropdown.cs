using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>Value dropdown for scriptable objects — finds all assets in the project, not just those in Resources.</summary>
[IncludeMyAttributes]
[ValueDropdown("@AssetDropdown.GetScriptableObjects($value)", AppendNextDrawer = true)]
[AssetsOnly]
public class AssetDropdown : Attribute
{
    private static readonly Dictionary<Type, (object[], DateTime)> cache = new();

    public static T[] GetScriptableObjects<T>(T scriptableObject) where T : ScriptableObject
    {
        var type = typeof(T);
        if (cache.TryGetValue(type, out var tuple) && (DateTime.Now - tuple.Item2).TotalSeconds < 1)
            return (T[])tuple.Item1;

#if UNITY_EDITOR
        var guids = UnityEditor.AssetDatabase.FindAssets("t:" + type.Name);
        var results = new List<T>(guids.Length);
        foreach (var guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
                results.Add(asset);
        }
        var arr = results.ToArray();
#else
        var arr = System.Array.Empty<T>();
#endif
        cache[type] = (arr, DateTime.Now);
        return arr;
    }
}
