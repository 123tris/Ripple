using System;
using System.Runtime.CompilerServices;
using UnityEngine;
#if RIPPLE_ULTEVENTS
using UltEvents;
#else
using UnityEngine.Events;
#endif

namespace Ripple
{
    public abstract class GameEvent : RippleStackTraceSO, IChannel
    {
        [SerializeField, HideInInspector] protected bool clearListenersOnPlaymode = true;

        public abstract int SubscriberCount { get; }
        public string Name => name;
        public abstract Type PayloadType { get; }
    }

    public class GameEvent<T> : GameEvent, IChannel<T>
    {
#if RIPPLE_ULTEVENTS
        [SerializeField] private UltEvent<T> response;
#else
        [SerializeField] private UnityEvent<T> response;
#endif

        private readonly Dispatcher<T> _dispatcher = new Dispatcher<T>();

        public override Type PayloadType => typeof(T);
        public override int SubscriberCount => _dispatcher.Count;

        protected void OnEnable()
        {
            hideFlags = HideFlags.DontUnloadUnusedAsset;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
        }

#if UNITY_EDITOR
        protected void OnDisable()
        {
            UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange state)
        {
            if (state != UnityEditor.PlayModeStateChange.ExitingPlayMode) return;
            ClearDebug();
            if (clearListenersOnPlaymode)
            {
                _dispatcher.Clear();
            }
        }
#endif

        public void Invoke(T value,
            [CallerMemberName] string callerMember = null,
            [CallerFilePath] string callerFile = null,
            [CallerLineNumber] int callerLine = 0)
        {
            LogInvoke(value, callerMember, callerFile, callerLine);

            using (RippleTraceContext.PushFrame(this, value, callerMember, callerFile, callerLine))
            {
                _dispatcher.Invoke(value);
                response?.Invoke(value);
            }
        }

        public IDisposable Subscribe(Action<T> handler)
        {
            _dispatcher.Add(handler);
            return new Subscription(this, handler);
        }

        public void Unsubscribe(Action<T> handler) => _dispatcher.Remove(handler);

        public void AddListener(Action<T> handler) => _dispatcher.Add(handler);
        public void RemoveListener(Action<T> handler) => _dispatcher.Remove(handler);

#if UNITY_EDITOR
        public Action<T>[] GetSubscribersDebug() => _dispatcher.Snapshot();
#endif

        private sealed class Subscription : IDisposable
        {
            private GameEvent<T> _event;
            private Action<T> _handler;
            public Subscription(GameEvent<T> e, Action<T> h) { _event = e; _handler = h; }
            public void Dispose()
            {
                if (_event == null) return;
                _event._dispatcher.Remove(_handler);
                _event = null;
                _handler = null;
            }
        }
    }
}

namespace Ripple
{
    [UnityEngine.CreateAssetMenu(menuName = "Ripple/Events/Void Event", fileName = "VoidEvent")]
    public sealed class VoidGameEvent : GameEvent<Unit>
    {
        public void Invoke() => Invoke(Unit.Default);
    }
}
