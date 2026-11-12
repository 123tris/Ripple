using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Ripple
{
    [InlineEditor]
    public class VariableListSO<T> : BaseVariable, IList<T>
    {
        [SerializeField, FormerlySerializedAs("_currentValue"), HideInPlayMode]
        private List<T> _initialValues = new();

        [NonSerialized, ShowInInspector, HideInEditorMode, LabelText("Values")]
        private List<T> _currentValue = new();

        public Action<T> OnItemAdded;
        public Action<T> OnItemRemoved;

        public List<T> CurrentValues => _currentValue;

        public override object Value => _currentValue;

        public int Count => _currentValue.Count;

        public bool IsReadOnly => false;

        public T this[int index]
        {
            get => _currentValue[index];
            set => _currentValue[index] = value;
        }

        public void Add(T item)
        {
            _currentValue.Add(item);
            OnItemAdded?.Invoke(item);
        }

        public void AddRange(IEnumerable<T> items)
        {
            foreach (T item in items)
                Add(item);
        }

        public void Insert(int index, T item)
        {
            _currentValue.Insert(index, item);
            OnItemAdded?.Invoke(item);
        }

        public bool Remove(T item)
        {
            if (!_currentValue.Remove(item)) return false;
            OnItemRemoved?.Invoke(item);
            return true;
        }

        public void RemoveAt(int index)
        {
            T item = _currentValue[index];
            _currentValue.RemoveAt(index);
            OnItemRemoved?.Invoke(item);
        }

        public void Resize(int count)
        {
            while (Count > count) RemoveAt(Count - 1);
            while (Count < count) Add(default);
        }

        public void Clear() => Resize(0);

        [Button, BoxGroup("Debug", order: 1), HideInInlineEditors]
        public override void ResetValue()
        {
            Clear();
            AddRange(_initialValues);
        }

        public bool Contains(T item) => _currentValue.Contains(item);

        public void CopyTo(T[] array, int arrayIndex) => _currentValue.CopyTo(array, arrayIndex);

        public int IndexOf(T item) => _currentValue.IndexOf(item);

        public IEnumerator<T> GetEnumerator() => _currentValue.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _currentValue.GetEnumerator();

        protected override void Restore()
        {
            _currentValue.Clear();
            _currentValue.AddRange(_initialValues);
        }
    }
}
