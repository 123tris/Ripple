using UnityEngine;
using UnityEngine.Scripting;

namespace Ripple
{
    [Preserve]
    [RippleData("Events")]
    [CreateAssetMenu(menuName = "Ripple/Events/String", fileName = "StringEvent")]
    public sealed class StringEvent : GameEvent<string> { }
}
