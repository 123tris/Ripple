using UnityEngine;
using UnityEngine.Scripting;

namespace Ripple
{
    [Preserve]
    [RippleData("Events")]
    [CreateAssetMenu(menuName = "Ripple/Events/GameObject", fileName = "GameObjectEvent")]
    public sealed class GameObjectEvent : GameEvent<GameObject> { }
}
