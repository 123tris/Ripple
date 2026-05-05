using UnityEngine;

namespace Ripple
{
    public abstract class AutoSubscribe<T> : MonoBehaviour
    {
        [SerializeField] protected GameEvent<T> _channel;

        protected virtual void OnEnable()
        {
            if (_channel != null) _channel.AddListener(OnRaised);
        }

        protected virtual void OnDisable()
        {
            if (_channel != null) _channel.RemoveListener(OnRaised);
        }

        protected abstract void OnRaised(T value);
    }
}
