using UnityEngine;

namespace Ripple
{
    [RippleData]
    [CreateAssetMenu(menuName = Config.EventMenu + "GameObject")]
    public class GameObjectEvent : GameEvent<GameObject> { }
}
