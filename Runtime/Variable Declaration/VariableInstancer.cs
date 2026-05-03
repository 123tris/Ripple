using System;
using System.Collections.Generic;
using System.Linq;
using Ripple;
using Sirenix.OdinInspector;
using UnityEngine;

public class VariableInstancer : MonoBehaviour
{
    [SerializeField] private List<VariableDeclaration> variables = new();


    [ShowInInspector, HideInEditorMode] private Dictionary<VariableDeclaration, object> instancedVariables = new();

    private void Awake()
    {
        foreach (var declaration in variables)
        {
            instancedVariables[declaration] = declaration.GetInitialValueAsObject();
        }
    }

    public object GetInstanceValue(VariableDeclaration variable)
    {
        if (instancedVariables.TryGetValue(variable, out object value))
            return value;

        Debug.LogError($"Cannot find instanced variable {variable.name} in object {name}");
        return null;
    }

    public T GetInstanceValue<T>(VariableDeclaration<T> variable)
    {
        return instancedVariables[variable] is T ? (T)instancedVariables[variable] : default;
    }

    public void SetInstanceValue(VariableDeclaration variable, object value)
    {
        instancedVariables[variable] = value;
    }

    public void SetInstanceValue<T>(VariableDeclaration<T> variable, T value)
    {
        instancedVariables[variable] = value;
    }
}