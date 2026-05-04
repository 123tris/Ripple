using Ripple;
using UltEvents;
using UnityEngine;

namespace Ripple
{
    [AddComponentMenu(Config.EventListenerMenu + "Event Listener Void")]
    public class EventListenerVoid : MonoBehaviour
    {
        [SerializeField]
        private VoidEventSO _event;

        [SerializeField]
        private UltEvent _response;

        void OnEnable()
        {
            if (_event == null)
            {
                Debug.LogWarning($"{nameof(EventListenerVoid)} on {name} has no event assigned.", this);
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

        private void OnEvent()
        {
            _response?.Invoke();
        }
    }
}