using System;
using System.Reflection;

namespace Ripple
{
    internal static class TypeExtension
    {
        public static Type ArgumentOf(this Type type, Type definition)
        {
            for (; type != null; type = type.BaseType)
                if (type.IsGenericType && type.GetGenericTypeDefinition() == definition)
                    return type.GetGenericArguments()[0];

            return null;
        }

        public static Type FieldTypeOf(this Type type, string name)
        {
            for (; type != null; type = type.BaseType)
            {
                FieldInfo field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                if (field != null) return field.FieldType;
            }

            return null;
        }
    }
}
