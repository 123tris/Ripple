using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

internal static class TypeExtension
{
    public static string[] GetSubclassNames(this Type type)
    {
        return Assembly.GetAssembly(type).DefinedTypes.Where(x => x.IsSubclassOf(type)).Select(x => x.ToString()).ToArray();
    }

    public static Type[] GetSubclasses(this Type type)
    {
        return Assembly.GetAssembly(type).DefinedTypes.Where(x => x.IsSubclassOf(type)).Select(x => x.DeclaringType).ToArray();
    }

    public static bool HasMethod(this Type type, string methodName)
    {
        return type.GetMethod(methodName) != null;
    }

    public static bool OverridesMethod(this Type type, string methodName)
    {
        MethodInfo method = type.GetMethod(
            methodName,
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        return method != null && method.DeclaringType == type;
    }

    public static bool IsSubclassOfRawGeneric(this Type toCheck,Type generic)
    {
        while (toCheck != null && toCheck != typeof(object))
        {
            var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
            if (generic == cur)
            {
                return true;
            }
            toCheck = toCheck.BaseType;
        }
        return false;
    }

    public static IEnumerable<TypeInfo> GetDerivedTypes<T>()
    {
        //Get all types that derive from the abstract class System Behaviour
        var definedTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.DefinedTypes);

        return definedTypes.Where(definedType => definedType.IsSubclassOf(typeof(T)) && !definedType.IsAbstract);
    }

    public static List<Transform> GetChildren(this Transform transform)
    {
        List<Transform> output = new List<Transform>();
        for (int i = 0; i < transform.childCount; i++)
        {
            output.Add(transform.GetChild(i));
        }
        return output;
    }

    public static void DestroyChildren(this GameObject gameObject)
    {
        foreach (Transform transform in gameObject.transform.GetChildren())
        {
            UnityEngine.Object.Destroy(transform.gameObject);
        }
    }

    public static IEnumerable<T> GetEnumValues<T>() where T : Enum
    {
        return Enum.GetValues(typeof(T)).Cast<T>();
    }
}