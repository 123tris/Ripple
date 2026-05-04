using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Ripple
{
    [RippleData]
    [CreateAssetMenu(menuName = Config.EventMenu + "Void")]
    public class VoidEventSO : GameEvent<Unit>
    {
        private readonly Dictionary<Action, Action<Unit>> _wrappers = new();

        [Button]
        public void Invoke()
        {
            Invoke(Unit.Default, null);
        }

        public void Invoke(UnityEngine.Object invoker)
        {
            Invoke(Unit.Default, invoker);
        }

        public void AddListener(Action method)
        {
            if (method == null) return;
            if (_wrappers.ContainsKey(method)) return;
            Action<Unit> wrapper = _ => method();
            _wrappers[method] = wrapper;
            AddListener(wrapper);
        }

        public void RemoveListener(Action method)
        {
            if (method == null) return;
            if (!_wrappers.TryGetValue(method, out var wrapper)) return;
            _wrappers.Remove(method);
            RemoveListener(wrapper);
        }
    }
}
