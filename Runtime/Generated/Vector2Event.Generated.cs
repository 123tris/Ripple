using UnityEngine;
using UnityEngine.Scripting;

namespace Ripple
{
    [Preserve]
    [RippleData("Events")]
    [CreateAssetMenu(menuName = "Ripple/Events/Vector2", fileName = "Vector2Event")]
    public sealed class Vector2Event : GameEvent<Vector2> { }
}
