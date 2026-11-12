using System;

internal static class TypeExtension
{
    public static bool IsSubclassOfRawGeneric(this Type toCheck, Type generic)
    {
        while (toCheck != null && toCheck != typeof(object))
        {
            Type current = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
            if (generic == current) return true;
            toCheck = toCheck.BaseType;
        }

        return false;
    }
}
