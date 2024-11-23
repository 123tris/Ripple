using System;
using Sirenix.OdinInspector;
using UnityEngine;

public class VariableSO<T> : ScriptableObject
{
#if UNITY_EDITOR
    [SerializeField, TextArea] private string _developerNotes;
#endif

    [SerializeField, ShowIf("@UnityEngine.Application.isPlaying")]
    private T _currentValue;

    [SerializeField, HideIf("@UnityEngine.Application.isPlaying")]
    private T _initialValue;

    private T _previousValue;

    public T CurrentValue
    {
        get => _currentValue;
        set {
            _previousValue = _currentValue;
            OnValueChanged?.Invoke(value);
            _currentValue = value;
        }
    }

    private T PreviousValue => _previousValue ?? default;

    public Action<T> OnValueChanged;

    [ShowInInspector, ShowIf("@UnityEngine.Application.isPlaying")]
    private Delegate[] ObjectsListeningToValueChanges => OnValueChanged?.GetInvocationList();

    void OnEnable()
    {
        _currentValue = _initialValue;
        CurrentValue = _initialValue;
    }
}

public interface INumericalVariable<T>
{
    public void Add(T value);
}