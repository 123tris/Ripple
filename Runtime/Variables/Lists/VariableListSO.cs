using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Ripple
{
    public abstract class VariableListSO : BaseVariable { }

    public class VariableListSO<T> : VariableListSO, IList<T>
    {
        [SerializeField, FormerlySerializedAs("_currentValue")]
        private List<T> _initialValues = new();

        [NonSerialized] private List<T> _currentValue = new();

        public Signal<T> OnItemAdded = new();
        public Signal<T> OnItemRemoved = new();

        public List<T> CurrentValues => _currentValue;

        public override object Value => _currentValue;

        public int Count => _currentValue.Count;

        public bool IsReadOnly => false;

        internal override IEnumerable<UnityEventBase> SerializedResponses => new UnityEventBase[] { OnItemAdded.Response, OnItemRemoved.Response };

        public T this[int index]
        {
            get => _currentValue[index];
            set
            {
                T replaced = _currentValue[index];
                _currentValue[index] = value;
                Removed(replaced);
                Added(value);
            }
        }

        public void Add(T item)
        {
            _currentValue.Add(item);
            Added(item);
        }

        public void AddRange(IEnumerable<T> items)
        {
            foreach (T item in items)
                Add(item);
        }

        public void Insert(int index, T item)
        {
            _currentValue.Insert(index, item);
            Added(item);
        }

        public bool Remove(T item)
        {
            if (!_currentValue.Remove(item)) return false;
            Removed(item);
            return true;
        }

        public void RemoveAt(int index)
        {
            T item = _currentValue[index];
            _currentValue.RemoveAt(index);
            Removed(item);
        }

        public void Resize(int count)
        {
            while (Count > count) RemoveAt(Count - 1);
            while (Count < count) Add(default);
        }

        public void Clear() => Resize(0);

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

        private void Added(T item)
        {
            Record("+ ", item);
            OnItemAdded.Invoke(item);
        }

        private void Removed(T item)
        {
            Record("- ", item);
            OnItemRemoved.Invoke(item);
        }

        private void Record(string change, T item)
        {
#if UNITY_EDITOR
            RecordInvoke(ResolveCaller(null), null, change + item);
#endif
        }
    }
}
