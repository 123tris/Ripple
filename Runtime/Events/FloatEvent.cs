using UnityEngine;
using Ripple;

namespace Ripple
{
    [RippleData]
    [CreateAssetMenu(menuName = Config.EventMenu + "Float")]
    public class FloatEvent : GameEvent<float> {}
}