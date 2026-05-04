using System;
using System.Linq;
using Ripple;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class RippleTypeCache
{
    private static Type[] CachedTypes = Type.EmptyTypes;
    public static bool IsReady = false;

    static RippleTypeCache()
    {
        Refresh();
        // Re-scan whenever scripts are reimported
        AssemblyReloadEvents.afterAssemblyReload += Refresh;
    }

    public static void Refresh()
    {
        CachedTypes = TypeCache.GetTypesWithAttribute<RippleData>()
            .Where(t => !t.IsAbstract && !t.IsGenericTypeDefinition)
            .OrderBy(t => t.Name)
            .ToArray();

        IsReady = true;

        EditorApplication.delayCall += () =>
        {
            var window = Resources.FindObjectsOfTypeAll<RippleWizard>().FirstOrDefault();
            if (window != null) window.ForceMenuTreeRebuild();
        };
    }

    public static bool TryGetCachedTypes(out Type[] types)
    {
        types = CachedTypes;
        return IsReady;
    }

    public static Type[] GetCachedTypes() => CachedTypes;
}
