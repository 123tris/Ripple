using System;
using UltEvents;
using UnityEngine;

[RippleData]
[CreateAssetMenu(menuName = "Events/Void")]
public class VoidEventSO : ScriptableObject
{
#if UNITY_EDITOR
    [SerializeField, TextArea] private string _developerNotes;
#endif

    [SerializeField]
    private UltEvent gameEvent;

    protected void OnEnable()
    {
        hideFlags = HideFlags.DontUnloadUnusedAsset;
    }

    public void Invoke()
    {
        gameEvent?.Invoke();
    }

    public bool HasListeners => gameEvent != null;

    public void AddListener(Action method) => gameEvent += method;

    public void RemoveListener(Action method) => gameEvent -= method;
}
