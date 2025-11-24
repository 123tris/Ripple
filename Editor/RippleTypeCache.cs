using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Ripple;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class RippleTypeCache
{
    private static Type[] CachedTypes = Type.EmptyTypes;
    public static bool IsReady = false;

    private const bool EnableDebugLogs = false;

    static RippleTypeCache()
    {
        RefreshAsync();
    }

    public static void RefreshAsync()
    {
        IsReady = false;

        Task.Run(() =>
        {
            var sw = Stopwatch.StartNew();

            try
            {
                var results = new List<Type>();
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                int assembliesScanned = 0;

                foreach (var assembly in assemblies)
                {
                    if (IsIgnoredAssembly(assembly)) continue;

                    assembliesScanned++;

                    try
                    {
                        foreach (var type in assembly.GetTypes())
                        {
                            if (type.IsDefined(typeof(RippleData), true))
                            {
                                results.Add(type);
                            }
                        }
                    }
                    catch (ReflectionTypeLoadException ex)
                    {
                        foreach (var type in ex.Types)
                        {
                            if (type != null && type.IsDefined(typeof(RippleData), true))
                            {
                                results.Add(type);
                            }
                        }
                    }
                }

                CachedTypes = results.OrderBy(t => t.Name).ToArray();
                IsReady = true;

                sw.Stop();

                Log($"Background Scan Finished.\n" +
                    $"Time: <b>{sw.ElapsedMilliseconds} ms</b> | " +
                    $"Found: {CachedTypes.Length} types | " +
                    $"Scanned: {assembliesScanned} assemblies");

                EditorApplication.delayCall += () =>
                {
                    var window = Resources.FindObjectsOfTypeAll<RippleWizard>().FirstOrDefault();
                    if (window != null) window.ForceMenuTreeRebuild();
                };
            }
            catch (Exception e)
            {
                sw.Stop();
                UnityEngine.Debug.LogError($"Failed fetching Ripple attributes after {sw.ElapsedMilliseconds}ms: {e.Message}");
                IsReady = true;
            }
        });
    }

    public static bool TryGetCachedTypes(out Type[] types)
    {
        types = IsReady ? CachedTypes : Type.EmptyTypes;
        return IsReady;
    }

    public static Type[] GetCachedTypes() => IsReady ? CachedTypes : Type.EmptyTypes;

    private static void Log(string message)
    {
        if (!EnableDebugLogs) return;

        UnityEngine.Debug.Log($"<color=cyan>[RippleCache]</color> {message}");
    }

    private static bool IsIgnoredAssembly(Assembly assembly)
    {
        string name = assembly.FullName;
        return name.StartsWith("Unity") ||
               name.StartsWith("System") ||
               name.StartsWith("mscorlib") ||
               name.StartsWith("Mono") ||
               name.StartsWith("JetBrains") ||
               name.StartsWith("Odin") ||
               name.StartsWith("nunit");
    }
}