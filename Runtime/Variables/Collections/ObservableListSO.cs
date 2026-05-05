using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ripple
{
    public abstract class ObservableListSO<T> : RippleStackTraceSO, ICollectionVariable<T>
    {
        [SerializeField] private List<T> _items = new();
        [SerializeField] private List<T> _initialItems = new();

        private readonly Dispatcher<T> _onAdded = new();
        private readonly Dispatcher<T> _onRemoved = new();
        private readonly Dispatcher<Unit> _onCleared = new();

        public int Count => _items.Count;
        public Type ItemType => typeof(T);
        public IReadOnlyList<T> Snapshot()
        {
            var arr = new T[_items.Count];
            _items.CopyTo(arr);
            return arr;
        }

        public T this[int index] => _items[index];

        public void ClearAll()
        {
            _items.Clear();
            _onCleared.Invoke(Unit.Default);
        }

        public void Add(T item)
        {
            _items.Add(item);
            _onAdded.Invoke(item);
        }

        public bool Remove(T item)
        {
            if (!_items.Remove(item)) return false;
            _onRemoved.Invoke(item);
            return true;
        }

        public IDisposable SubscribeAdded(Action<T> h) { _onAdded.Add(h); return new Sub(_onAdded, h); }
        public IDisposable SubscribeRemoved(Action<T> h) { _onRemoved.Add(h); return new Sub(_onRemoved, h); }
        public IDisposable SubscribeCleared(Action<Unit> h) { _onCleared.Add(h); return new SubU(_onCleared, h); }

        protected virtual void OnEnable()
        {
            hideFlags = HideFlags.DontUnloadUnusedAsset;
            _items.Clear();
            _items.AddRange(_initialItems);
        }

        private sealed class Sub : IDisposable
        {
            private Dispatcher<T> _d; private Action<T> _h;
            public Sub(Dispatcher<T> d, Action<T> h) { _d = d; _h = h; }
            public void Dispose() { _d?.Remove(_h); _d = null; _h = null; }
        }

        private sealed class SubU : IDisposable
        {
            private Dispatcher<Unit> _d; private Action<Unit> _h;
            public SubU(Dispatcher<Unit> d, Action<Unit> h) { _d = d; _h = h; }
            public void Dispose() { _d?.Remove(_h); _d = null; _h = null; }
        }
    }

    [RippleData("Collections")]
    [CreateAssetMenu(menuName = "Ripple/Collections/Int List", fileName = "IntList")]
    public sealed class IntListSO : ObservableListSO<int> { }

    [RippleData("Collections")]
    [CreateAssetMenu(menuName = "Ripple/Collections/Float List", fileName = "FloatList")]
    public sealed class FloatListSO : ObservableListSO<float> { }

    [RippleData("Collections")]
    [CreateAssetMenu(menuName = "Ripple/Collections/String List", fileName = "StringList")]
    public sealed class StringListSO : ObservableListSO<string> { }

    [RippleData("Collections")]
    [CreateAssetMenu(menuName = "Ripple/Collections/GameObject List", fileName = "GameObjectList")]
    public sealed class GameObjectListSO : ObservableListSO<UnityEngine.GameObject> { }
}
