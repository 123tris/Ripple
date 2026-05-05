using UnityEngine;
using UnityEngine.Scripting;

namespace Ripple
{
    [Preserve]
    [RippleData("Events")]
    [CreateAssetMenu(menuName = "Ripple/Events/Transform", fileName = "TransformEvent")]
    public sealed class TransformEvent : GameEvent<Transform> { }
}
