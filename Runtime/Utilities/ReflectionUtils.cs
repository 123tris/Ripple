using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public static class ReflectionUtils
{
    public static List<Type> GetTypesWithAttribute<TAttribute>(bool inherit = false) 
        where TAttribute : Attribute
    {
        var output = new List<Type>();

        // 1. Get all loaded assemblies
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        // 2. Scan each assembly
        foreach (var assembly in assemblies)
        {
            // OPTIONAL: Skip obvious system/unity assemblies to speed this up significantly.
            // See the "Optimization" section below for details.
            if (IsIgnoredAssembly(assembly)) continue;

            try
            {
                // 3. Get all types in the assembly
                var types = assembly.GetTypes();

                foreach (var type in types)
                {
                    // 4. Check if the type has the attribute
                    // 'inherit' parameter determines if we check parent classes
                    if (type.IsDefined(typeof(TAttribute), inherit))
                    {
                        output.Add(type);
                    }
                }
            }
            catch (ReflectionTypeLoadException e)
            {
                // 5. CRITICAL: Handle types that failed to load.
                // In Unity, this happens often with plugins or editor scripts.
                // The exception contains the partial list of types that *did* succeed.
                foreach (var type in e.Types)
                {
                    if (type != null && type.IsDefined(typeof(TAttribute), inherit))
                    {
                        output.Add(type);
                    }
                }
            }
            catch (Exception e)
            {
                // Log other errors but keep scanning other assemblies
                UnityEngine.Debug.LogWarning($"Failed to scan assembly {assembly.FullName}: {e.Message}");
            }
        }

        return output;
    }

    // Basic filter to skip scanning 10,000+ System/Unity types
    private static bool IsIgnoredAssembly(Assembly assembly)
    {
        string name = assembly.FullName;
        
        return name.StartsWith("Unity") || 
               name.StartsWith("System") || 
               name.StartsWith("mscorlib") || 
               name.StartsWith("Mono") ||
               name.StartsWith("nunit");
    }
}