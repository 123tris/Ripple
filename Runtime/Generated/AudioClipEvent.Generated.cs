using UnityEngine;
using UnityEngine.Scripting;

namespace Ripple
{
    [Preserve]
    [RippleData("Events")]
    [CreateAssetMenu(menuName = "Ripple/Events/AudioClip", fileName = "AudioClipEvent")]
    public sealed class AudioClipEvent : GameEvent<AudioClip> { }
}
