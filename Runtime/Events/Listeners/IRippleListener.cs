using System;

namespace Ripple
{
    /// <summary>
    /// Implemented by Ripple listener components (<see cref="EventListener{T}"/>,
    /// <see cref="EventListenerVoid"/>, <see cref="VariableListener{T}"/>) so editor tooling can
    /// enumerate which event/variable asset a component listens to without reflecting over its
    /// private serialized fields.
    /// </summary>
    public interface IRippleListener
    {
        /// <summary>The event or variable asset this component listens to, or null if unassigned.</summary>
        RippleStackTraceSO Target { get; }

        /// <summary>The value type this listener responds with (<c>typeof(void)</c> for the void listener).</summary>
        Type ResponseValueType { get; }
    }
}
