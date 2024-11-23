using UltEvents;
using UnityEngine;

public class EventListener<T> : MonoBehaviour
{
    [SerializeField]
    private GameEvent<T> _event;

    [SerializeField]
    private UltEvent<T> _response;

    void OnEnable() => _event.AddListener(OnEvent);

    void OnDisable() => _event.RemoveListener(OnEvent);

    private void OnEvent(T value)
    {
        _response?.Invoke(value);
    }
}