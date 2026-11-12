using System;

namespace Ripple
{
    public interface IRippleListener
    {
        RippleStackTraceSO Target { get; }

        Type ResponseValueType { get; }
    }
}
