using UltEvents;
using UnityEngine;

namespace Ripple
{
    public class EventListener<T> : MonoBehaviour
    {
        [ListenerReference]
        [SerializeField]
        private GameEvent<T> _event;

        [SerializeField]
        private UltEvent<T> _response;

        void OnEnable()
        {
            if (_event == null)
            {
                Debug.LogWarning($"{nameof(EventListener<T>)} on {name} has no event assigned.", this);
                return;
            }

            _event.AddListener(OnEvent);
        }

        void OnDisable()
        {
            if (_event == null)
                return;
            _event.RemoveListener(OnEvent);
        }

        private void OnEvent(T value)
        {
            _response?.Invoke(value);
        }
    }
}