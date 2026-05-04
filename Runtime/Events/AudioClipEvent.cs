using UnityEngine;

namespace Ripple
{
    [RippleData]
    [CreateAssetMenu(fileName = "Audio Clip Event", menuName = Config.EventMenu + "AudioClip")]
    public class AudioClipEvent : GameEvent<AudioClip> { }
}
