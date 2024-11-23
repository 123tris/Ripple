using Ripple;
using UltEvents;
using UnityEngine;

namespace Ripple
{
    public class EventListenerVoid : MonoBehaviour
    {
        [SerializeField]
        private VoidEventSO _event;

        [SerializeField]
        private UltEvent _response;

        void OnEnable() => _event.AddListener(OnEvent);

        void OnDisable() => _event.RemoveListener(OnEvent);

        private void OnEvent()
        {
            _response?.Invoke();
        }
    }
}