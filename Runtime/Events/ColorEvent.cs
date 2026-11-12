using UnityEngine;

namespace Ripple
{
    [RippleData]
    [CreateAssetMenu(menuName = Config.EventMenu + "Color")]
    public class ColorEvent : GameEvent<Color> { }
}
