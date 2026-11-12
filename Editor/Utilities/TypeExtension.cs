using System;

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
    }
}
