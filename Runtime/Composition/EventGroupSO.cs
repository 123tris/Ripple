using System.Collections.Generic;
using UnityEngine;

namespace Ripple
{
    [RippleData("Composition")]
    [CreateAssetMenu(menuName = "Ripple/Composition/Event Group", fileName = "EventGroup")]
    public sealed class EventGroupSO : RippleStackTraceSO, IChannel
    {
        [SerializeField] private List<VoidGameEvent> _children = new();

        public string Name => name;
        public System.Type PayloadType => typeof(Unit);
        public int SubscriberCount
        {
            get
            {
                int total = 0;
                foreach (var c in _children) if (c != null) total += c.SubscriberCount;
                return total;
            }
        }

        public void RaiseAll()
        {
            using (RippleTraceContext.PushFrame(this, Unit.Default, nameof(RaiseAll), GetType().Name, 0))
            {
                foreach (var c in _children)
                    if (c != null) c.Invoke();
            }
        }

    }
}
