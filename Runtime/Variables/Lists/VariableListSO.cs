using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Ripple
{
    [RippleData]
    [InlineEditor]
    public class VariableListSO<T> : RippleStackTraceSO, IList<T>
    {
        [SerializeField]
        private List<T> _currentValue = new();

        [SerializeField, HideInPlayMode]
        private List<T> _initialValue = new();

        public List<T> CurrentValues => _currentValue;

        public Action<T> OnItemAdded;
        public Action<T> OnItemRemoved;
        public Action<int, T> OnItemChanged;
        public Action OnCleared;

#if UNITY_EDITOR
        protected void OnEnable()
        {
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        protected void OnDisable()
        {
            UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        }

        private void OnPlayModeChanged(UnityEditor.PlayModeStateChange state)
        {
            if (state == UnityEditor.PlayModeStateChange.ExitingEditMode)
                ResetToInitial();
        }
#endif

        public void ResetToInitial()
        {
            _currentValue = new List<T>(_initialValue);
            OnCleared?.Invoke();
        }

        public IEnumerator<T> GetEnumerator() => _currentValue.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_currentValue).GetEnumerator();

        public void Add(T item)
        {
            _currentValue.Add(item);
            OnItemAdded?.Invoke(item);
        }

        public void Clear()
        {
            _currentValue.Clear();
            OnCleared?.Invoke();
        }

        public bool Contains(T item) => _currentValue.Contains(item);

        public void CopyTo(T[] array, int arrayIndex) => _currentValue.CopyTo(array, arrayIndex);

        public bool Remove(T item)
        {
            bool removed = _currentValue.Remove(item);
            if (removed)
                OnItemRemoved?.Invoke(item);
            return removed;
        }

        public int Count => _currentValue.Count;

        public bool IsReadOnly => (_currentValue as IList<T>).IsReadOnly;

        public int IndexOf(T item) => _currentValue.IndexOf(item);

        public void Insert(int index, T item)
        {
            _currentValue.Insert(index, item);
            OnItemAdded?.Invoke(item);
        }

        public void RemoveAt(int index)
        {
            T item = _currentValue[index];
            _currentValue.RemoveAt(index);
            OnItemRemoved?.Invoke(item);
        }

        public T this[int index]
        {
            get => _currentValue[index];
            set
            {
                _currentValue[index] = value;
                OnItemChanged?.Invoke(index, value);
            }
        }
    }
}
