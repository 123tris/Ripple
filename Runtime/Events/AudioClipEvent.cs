using UnityEngine;

namespace Ripple
{
    [RippleData]
    [CreateAssetMenu(menuName = Config.EventMenu + "AudioClip")]
    public class AudioClipEvent : GameEvent<AudioClip> { }
}
