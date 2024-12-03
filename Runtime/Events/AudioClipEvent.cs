using UnityEngine;


namespace Ripple
{
    [CreateAssetMenu(fileName = "Audio Clip Event", menuName = Config.EventMenu + "AudioClip")]

    public class AudioClipEvent : GameEvent<AudioClip> { }
}
