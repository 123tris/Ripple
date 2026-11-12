using UnityEngine;

namespace Ripple
{
    [RippleData]
    [CreateAssetMenu(menuName = Config.EventMenu + "String")]
    public class StringEvent : GameEvent<string> { }
}
