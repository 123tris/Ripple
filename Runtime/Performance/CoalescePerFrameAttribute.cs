using System;

namespace Ripple
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class CoalescePerFrameAttribute : Attribute { }
}
