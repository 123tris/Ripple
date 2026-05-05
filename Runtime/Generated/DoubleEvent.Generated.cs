using UnityEngine;
using UnityEngine.Scripting;

namespace Ripple
{
    [Preserve]
    [RippleData("Events")]
    [CreateAssetMenu(menuName = "Ripple/Events/Double", fileName = "DoubleEvent")]
    public sealed class DoubleEvent : GameEvent<double> { }
}
