using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Ripple.Editor
{
    public static class RippleTypeCache
    {
        private static Type[] _rippleDataTypes;
        private static Task _loadTask;

        [InitializeOnLoadMethod]
        private static void Init()
        {
            _loadTask = Task.Run(Scan);
        }

        public static Type[] GetRippleDataTypes()
        {
            if (_rippleDataTypes != null) return _rippleDataTypes;
            _loadTask?.Wait();
            return _rippleDataTypes ?? Array.Empty<Type>();
        }

        public static IEnumerable<IGrouping<string, Type>> Grouped()
        {
            return GetRippleDataTypes().GroupBy(t =>
            {
                var attr = t.GetCustomAttribute<RippleDataAttribute>();
                return attr?.Group ?? "Other";
            });
        }

        private static void Scan()
        {
            var types = new List<Type>();
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] asmTypes;
                try { asmTypes = asm.GetTypes(); }
                catch (ReflectionTypeLoadException ex) { asmTypes = ex.Types.Where(t => t != null).ToArray(); }
                foreach (var t in asmTypes)
                {
                    if (t == null || t.IsAbstract) continue;
                    if (!typeof(ScriptableObject).IsAssignableFrom(t)) continue;
                    if (t.GetCustomAttribute<RippleDataAttribute>() == null) continue;
                    types.Add(t);
                }
            }
            _rippleDataTypes = types.OrderBy(t => t.Name).ToArray();
        }
    }
}
