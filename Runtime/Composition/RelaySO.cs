using System;
using UnityEngine;

namespace Ripple
{
    public abstract class RelaySO<TIn, TOut> : RippleStackTraceSO
    {
        [SerializeField] private GameEvent<TIn> _source;
        [SerializeField] private GameEvent<TOut> _target;

        private IDisposable _subscription;

        protected virtual void OnEnable()
        {
            if (_source != null && _target != null)
                _subscription = _source.Subscribe(OnSourceRaised);
        }

        protected virtual void OnDisable()
        {
            _subscription?.Dispose();
            _subscription = null;
        }

        private void OnSourceRaised(TIn value)
        {
            if (_target == null) return;
            if (TryTransform(value, out var output))
                _target.Invoke(output);
        }

        protected abstract bool TryTransform(TIn input, out TOut output);
    }
}
