using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Ripple
{
    [InlineEditor]
    public class VariableListSO<T> : RippleStackTraceSO, IList<T>
    {
        [SerializeField] private List<T> _currentValue = new();

        [SerializeField, Tooltip("Start every play session empty. Use for sets that scene objects join at runtime.")]
        private bool _runtimeSet;

        public Action<T> OnItemAdded;
        public Action<T> OnItemRemoved;

        public List<T> CurrentValues => _currentValue;

        public int Count => _currentValue.Count;

        public bool IsReadOnly => false;

        public T this[int index]
        {
            get => _currentValue[index];
            set => _currentValue[index] = value;
        }

        private void OnEnable()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
            ResetRuntimeSet();
        }

#if UNITY_EDITOR
        private void OnDisable() => UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;

        private void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange state)
        {
            if (state != UnityEditor.PlayModeStateChange.ExitingEditMode) return;
            ResetRuntimeSet();
            ClearInvokers();
        }
#endif

        private void ResetRuntimeSet()
        {
            if (_runtimeSet) _currentValue.Clear();
        }

        public void Add(T item)
        {
            _currentValue.Add(item);
            OnItemAdded?.Invoke(item);
        }

        public bool Remove(T item)
        {
            if (!_currentValue.Remove(item)) return false;
            OnItemRemoved?.Invoke(item);
            return true;
        }

        public void Clear() => _currentValue.Clear();

        public bool Contains(T item) => _currentValue.Contains(item);

        public void CopyTo(T[] array, int arrayIndex) => _currentValue.CopyTo(array, arrayIndex);

        public int IndexOf(T item) => _currentValue.IndexOf(item);

        public void Insert(int index, T item) => _currentValue.Insert(index, item);

        public void RemoveAt(int index) => _currentValue.RemoveAt(index);

        public IEnumerator<T> GetEnumerator() => _currentValue.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _currentValue.GetEnumerator();
    }
}
