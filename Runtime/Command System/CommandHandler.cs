using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Ripple.Experimental
{
    public class CommandHandler : MonoBehaviour
    {
        [System.Flags]
        private enum EventType
        {
            Awake = 1,
            Start = 2,
            OnEnable = 4,
            OnDisable = 8,
            OnDestroy = 16,
            OnUpdate = 32
        }

        [SerializeField] private EventType eventType;

        [SerializeReference] List<ICommand> commands = new();

        [Button]
        public void Execute()
        {
            foreach (var command in commands)
            {
                command.Execute(gameObject);
            }
        }

        void Awake()
        {
            if (eventType.HasFlag(EventType.Awake))
                Execute();
        }

        void Start()
        {
            if (eventType.HasFlag(EventType.Start))
                Execute();
        }

        void OnEnable()
        {
            if (eventType.HasFlag(EventType.OnEnable))
                Execute();
        }

        void OnDisable()
        {
            if (eventType.HasFlag(EventType.OnDisable))
                Execute();
        }

        private void OnDestroy()
        {
            if (eventType.HasFlag(EventType.OnDestroy))
                Execute();
        }

        private void Update()
        {
            if  (eventType.HasFlag(EventType.OnUpdate))
                Execute();
        }
    }
}