using System;

namespace Ripple
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class RippleDataAttribute : Attribute
    {
        public string Group { get; }
        public RippleDataAttribute(string group = null) { Group = group; }
    }
}
