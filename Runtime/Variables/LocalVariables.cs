using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ripple
{
    [AddComponentMenu("Ripple/Local Variables"), DisallowMultipleComponent]
    public class LocalVariables : MonoBehaviour
    {
        [SerializeField, Tooltip("This object and its children get their own copy of each variable listed here.")]
        private BaseVariable[] variables = Array.Empty<BaseVariable>();

        private readonly Dictionary<BaseVariable, BaseVariable> copies = new();

        public T Get<T>(T variable) where T : BaseVariable
        {
            if (Array.IndexOf(variables, variable) < 0) return variable;
            if (copies.TryGetValue(variable, out BaseVariable copy)) return (T)copy;

            copy = Instantiate(variable);
            copy.name = $"{variable.name} ({name})";
            copies.Add(variable, copy);
            return (T)copy;
        }

        private void OnDestroy()
        {
            foreach (BaseVariable copy in copies.Values)
                Destroy(copy);
        }
    }

    public static class LocalVariableExtensions
    {
        public static T For<T>(this T variable, Component owner) where T : BaseVariable
        {
            var local = owner.GetComponentInParent<LocalVariables>();
            return local ? local.Get(variable) : variable;
        }
    }
}
