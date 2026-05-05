using UnityEngine;
using UnityEngine.Scripting;

namespace Ripple
{
    [Preserve]
    [RippleData("Events")]
    [CreateAssetMenu(menuName = "Ripple/Events/Float", fileName = "FloatEvent")]
    public sealed class FloatEvent : GameEvent<float> { }
}
