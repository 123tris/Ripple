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
        [SerializeField, HideInInspector]
        private List<T> _currentValue;

        [ShowInInspector]
        public List<T> CurrentValues => _currentValue;

        public Action<T> OnValueAdded;
        public Action<T> OnValueRemoved;
        
        public IEnumerator<T> GetEnumerator()
        {
            return _currentValue.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_currentValue).GetEnumerator();
        }

        public void Add(T item)
        {
            OnValueAdded?.Invoke(item);
            _currentValue.Add(item);
        }

        public void Clear()
        {
            _currentValue.Clear();
        }

        public bool Contains(T item)
        {
            return _currentValue.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _currentValue.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            OnValueRemoved?.Invoke(item);
            return _currentValue.Remove(item);
        }

        public int Count => _currentValue.Count;

        public bool IsReadOnly => (_currentValue as IList<T>).IsReadOnly;

        public int IndexOf(T item)
        {
            return _currentValue.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            _currentValue.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _currentValue.RemoveAt(index);
        }

        public T this[int index]
        {
            get => _currentValue[index];
            set => _currentValue[index] = value;
        }
    }
}
