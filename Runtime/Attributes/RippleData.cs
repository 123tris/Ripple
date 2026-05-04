using System;

namespace Ripple
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class RippleData : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class ListenerReferenceAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class InvokerReferenceAttribute : Attribute
    {
    }
}