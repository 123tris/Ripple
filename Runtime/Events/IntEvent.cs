using UnityEngine;
using Ripple;

namespace Ripple
{
    [RippleData]
    [CreateAssetMenu(menuName = Config.EventMenu + "Int")]
    public class IntEvent : GameEvent<int> { }
}