using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
#if ULTEVENTS
using UltEvents;
#else
using UnityEngine.Events;
#endif

namespace Ripple
{
    [RippleData]
    [InlineEditor]
    public class GameEvent<T> : GameEvent
    {
        [FormerlySerializedAs("gameEvent")]
        [SerializeField]
#if ULTEVENTS
        private UltEvent<T> response;
#else
        private UnityEvent<T> response;
#endif

        private readonly List<WeakReference<Action<T>>> _runtimeListeners = new();
        private readonly HashSet<Action<T>> _runtimeListenerSet = new();

        protected override void OnEnable()
        {
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
        }

        [Button, BoxGroup("Debug")]
        public virtual void Invoke(T parameter)
        {
            Invoke(parameter, null);
        }

        public virtual void Invoke(T parameter, UnityEngine.Object invoker)
        {
            PurgeDeadListeners();
            LogInvoke(parameter, invoker);
            if (response == null) return;

#if ULTEVENTS
            if (!response.HasCalls)
                Debug.LogWarning($"Calling event \"{name}\" but no listeners were found", this);
#endif

            try
            {
                response.Invoke(parameter);
            }
            catch (Exception exception)
            {
#if UNITY_EDITOR
                Logger.Log(new LogMetadata
                {
                    Message = $"Exception while invoking <color=yellow>{name}</color> with value <color=green>{parameter}</color>",
                    Context = this,
                    Invoker = invoker,
                    Caller = exception.TargetSite?.Name ?? "unknown",
                    FullStackTrace = exception.StackTrace ?? string.Empty,
                    HasException = true,
                    ExceptionDetails = exception.ToString(),
                    SourceKind = LogSourceKind.EventInvoke
                });
#endif
                throw;
            }
        }

        public void Invoke(VariableSO<T> parameter)
        {
            Invoke(parameter.CurrentValue);
        }

        public bool HasListeners => response != null
#if ULTEVENTS
            && response.HasCalls
#else
            && response.GetPersistentEventCount() > 0
#endif
            ;

        public override int RuntimeListenerCount => _runtimeListenerSet.Count;

        public void AddListener(Action<T> method)
        {
            if (response == null || method == null)
                return;
            if (_runtimeListenerSet.Contains(method))
                return;
#if ULTEVENTS
            response += method;
#else
            response.AddListener(new UnityAction<T>(method));
#endif
            _runtimeListeners.Add(new WeakReference<Action<T>>(method));
            _runtimeListenerSet.Add(method);
        }

        public void RemoveListener(Action<T> method)
        {
            if (response == null || method == null)
                return;
#if ULTEVENTS
            response -= method;
#else
            response.RemoveListener(new UnityAction<T>(method));
#endif
            _runtimeListeners.RemoveAll(wr => !wr.TryGetTarget(out var t) || t == method);
            _runtimeListenerSet.Remove(method);
        }

        public override void AddGenericListener(Delegate method)
        {
#if ULTEVENTS
            response.AddPersistentCall(method);
#endif
        }

        public override void RemoveGenericListener(Delegate method)
        {
#if ULTEVENTS
            response.RemovePersistentCall(method);
#endif
        }

        protected override void ClearRuntimeListeners()
        {
            if (response == null || _runtimeListenerSet.Count == 0)
                return;

            foreach (var listener in _runtimeListenerSet)
            {
#if ULTEVENTS
                response -= listener;
#else
                response.RemoveListener(new UnityAction<T>(listener));
#endif
            }
            _runtimeListeners.Clear();
            _runtimeListenerSet.Clear();
        }

        private void PurgeDeadListeners()
        {
            for (int i = _runtimeListeners.Count - 1; i >= 0; i--)
            {
                if (!_runtimeListeners[i].TryGetTarget(out var target))
                {
                    _runtimeListeners.RemoveAt(i);
                }
            }
        }

        protected override void ClearPersistentListeners()
        {
            if (response != null)
#if ULTEVENTS
                response.Clear();
#else
                response.RemoveAllListeners();
#endif
        }
    }

    public abstract class GameEvent : RippleStackTraceSO
    {
#if UNITY_EDITOR
        private Action<UnityEditor.PlayModeStateChange> _onPlayModeChanged;
#endif

        [SerializeField, BoxGroup("Debug")] protected bool clearListenersOnPlaymode;

        [SerializeField, Tooltip("Optional group for organising this event in the Ripple Wizard.")]
        private EventGroupSO _group;

        public EventGroupSO Group => _group;

        protected virtual void OnEnable()
        {
#if UNITY_EDITOR
            _onPlayModeChanged = state =>
            {
                if (state == UnityEditor.PlayModeStateChange.ExitingEditMode && clearListenersOnPlaymode)
                    ClearAllListeners();
            };
            UnityEditor.EditorApplication.playModeStateChanged += _onPlayModeChanged;
#endif
            hideFlags = HideFlags.DontUnloadUnusedAsset;
        }

        protected virtual void OnDisable()
        {
#if UNITY_EDITOR
            if (_onPlayModeChanged != null)
                UnityEditor.EditorApplication.playModeStateChanged -= _onPlayModeChanged;
#endif
            if (clearListenersOnPlaymode)
                ClearRuntimeListeners();
        }

        public abstract void AddGenericListener(Delegate method);
        public abstract void RemoveGenericListener(Delegate method);
        public abstract int RuntimeListenerCount { get; }

        protected abstract void ClearRuntimeListeners();
        protected abstract void ClearPersistentListeners();

        protected void ClearAllListeners()
        {
            ClearRuntimeListeners();
            ClearPersistentListeners();
        }
    }
}
