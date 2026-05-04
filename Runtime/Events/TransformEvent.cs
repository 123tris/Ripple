using UnityEngine;

namespace Ripple
{
    [RippleData]
    [CreateAssetMenu(menuName = Config.EventMenu + "Transform")]
    public class TransformEvent : GameEvent<Transform>
    {
    }
}
