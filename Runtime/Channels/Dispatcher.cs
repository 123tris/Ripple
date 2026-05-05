using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ripple
{
    public sealed class Dispatcher<T>
    {
        private Action<T>[] _listeners = Array.Empty<Action<T>>();
        private int _count;
        private bool _dispatching;
        private List<Action<T>> _pendingAdd;
        private List<Action<T>> _pendingRemove;

        public int Count => _count;

        public void Add(Action<T> handler)
        {
            if (handler == null) return;
            if (_dispatching)
            {
                (_pendingAdd ??= new List<Action<T>>()).Add(handler);
                return;
            }
            EnsureCapacity(_count + 1);
            _listeners[_count++] = handler;
        }

        public void Remove(Action<T> handler)
        {
            if (handler == null) return;
            if (_dispatching)
            {
                (_pendingRemove ??= new List<Action<T>>()).Add(handler);
                return;
            }
            for (int i = 0; i < _count; i++)
            {
                if (_listeners[i].Equals(handler))
                {
                    _listeners[i] = _listeners[--_count];
                    _listeners[_count] = null;
                    return;
                }
            }
        }

        public void Invoke(T value)
        {
            int count = _count;
            var snapshot = _listeners;
            _dispatching = true;
            try
            {
                for (int i = 0; i < count; i++)
                {
                    try { snapshot[i].Invoke(value); }
                    catch (Exception e) { Debug.LogException(e); }
                }
            }
            finally
            {
                _dispatching = false;
                FlushPending();
            }
        }

        public void Clear()
        {
            if (_dispatching)
            {
                for (int i = 0; i < _count; i++)
                    (_pendingRemove ??= new List<Action<T>>()).Add(_listeners[i]);
                return;
            }
            for (int i = 0; i < _count; i++) _listeners[i] = null;
            _count = 0;
        }

        public Action<T>[] Snapshot()
        {
            var result = new Action<T>[_count];
            Array.Copy(_listeners, result, _count);
            return result;
        }

        private void FlushPending()
        {
            if (_pendingRemove != null)
            {
                for (int i = 0; i < _pendingRemove.Count; i++) Remove(_pendingRemove[i]);
                _pendingRemove.Clear();
            }
            if (_pendingAdd != null)
            {
                for (int i = 0; i < _pendingAdd.Count; i++) Add(_pendingAdd[i]);
                _pendingAdd.Clear();
            }
        }

        private void EnsureCapacity(int required)
        {
            if (_listeners.Length >= required) return;
            int newSize = Math.Max(4, _listeners.Length * 2);
            while (newSize < required) newSize *= 2;
            Array.Resize(ref _listeners, newSize);
        }
    }
}
