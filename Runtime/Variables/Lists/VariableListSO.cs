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
        [SerializeField]
        private List<T> _currentValue;

        public List<T> CurrentValues => _currentValue;

        public Action<T> OnItemAdded;
        public Action<T> OnItemRemoved;

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
            OnItemAdded?.Invoke(item);
            _currentValue.Add(item);
        }

        public void AddRange(IEnumerable<T> items)
        {
            _currentValue.AddRange(items);
        }

        public void ResizeRange(int count)
        {
            if (count < 0)
            {
                count = 0;
                Debug.LogError("Count cannot be negative. Setting count to 0.");
            }
            if (_currentValue.Count > count)
            {
                _currentValue.RemoveRange(count, _currentValue.Count - count);
            }
            else if (_currentValue.Count < count)
            {
                _currentValue.AddRange(new T[count - _currentValue.Count]);
            }
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
            OnItemRemoved?.Invoke(item);
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
