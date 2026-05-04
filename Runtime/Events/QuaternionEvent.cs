using UnityEngine;

namespace Ripple
{
    [RippleData]
    [CreateAssetMenu(menuName = Config.EventMenu + "Quaternion")]
    public class QuaternionEvent : GameEvent<Quaternion> { }
}
