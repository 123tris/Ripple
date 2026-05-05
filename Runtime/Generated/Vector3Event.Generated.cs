using UnityEngine;
using UnityEngine.Scripting;

namespace Ripple
{
    [Preserve]
    [RippleData("Events")]
    [CreateAssetMenu(menuName = "Ripple/Events/Vector3", fileName = "Vector3Event")]
    public sealed class Vector3Event : GameEvent<Vector3> { }
}
