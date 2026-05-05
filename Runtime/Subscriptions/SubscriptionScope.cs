using System;
using System.Collections.Generic;

namespace Ripple
{
    public sealed class SubscriptionScope : IDisposable
    {
        private List<IDisposable> _subscriptions = new List<IDisposable>();

        public void Add(IDisposable subscription)
        {
            if (subscription == null) return;
            _subscriptions.Add(subscription);
        }

        public IDisposable Subscribe<T>(IChannel<T> channel, Action<T> handler)
        {
            var s = channel.Subscribe(handler);
            _subscriptions.Add(s);
            return s;
        }

        public void Dispose()
        {
            if (_subscriptions == null) return;
            for (int i = _subscriptions.Count - 1; i >= 0; i--)
            {
                try { _subscriptions[i]?.Dispose(); }
                catch (Exception e) { UnityEngine.Debug.LogException(e); }
            }
            _subscriptions.Clear();
            _subscriptions = null;
        }
    }
}
