using UnityEngine;
using Ripple;

namespace Ripple
{
    [RippleData]
    [CreateAssetMenu(menuName = Config.EventMenu + "Bool")]
    public class BoolEvent : GameEvent<bool> { }
}