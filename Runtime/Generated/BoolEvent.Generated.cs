using UnityEngine;
using UnityEngine.Scripting;

namespace Ripple
{
    [Preserve]
    [RippleData("Events")]
    [CreateAssetMenu(menuName = "Ripple/Events/Bool", fileName = "BoolEvent")]
    public sealed class BoolEvent : GameEvent<bool> { }
}
