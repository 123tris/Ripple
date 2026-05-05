using UnityEngine;
using UnityEngine.Scripting;

namespace Ripple
{
    [Preserve]
    [RippleData("Events")]
    [CreateAssetMenu(menuName = "Ripple/Events/Int", fileName = "IntEvent")]
    public sealed class IntEvent : GameEvent<int> { }
}
