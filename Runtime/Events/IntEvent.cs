using UnityEngine;
using Ripple;

namespace Ripple
{
    [RippleData]
    [CreateAssetMenu(menuName = Config.EventMenu + "Integer")]
    public class IntEvent : GameEvent<int> { }
}