using System.Collections.Generic;
using UnityEngine;

namespace Ripple
{
    internal class RipplePreviewData : ScriptableObject
    {
        public List<RippleStackTraceSO.InvokeRecord> History = new();
        public List<RippleConnections.Listener> Listeners = new();
        public List<LogEntry> Log = new();
    }
}
