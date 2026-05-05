using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Ripple.Editor
{
    [InitializeOnLoad]
    public static class ListenerLeakDetector
    {
        private static readonly Dictionary<int, List<int>> _historyByInstanceId = new();
        private const int SessionsToFlag = 5;

        static ListenerLeakDetector()
        {
            EditorApplication.playModeStateChanged += state =>
            {
                if (state == PlayModeStateChange.EnteredEditMode) Snapshot();
            };
        }

        private static void Snapshot()
        {
            var allChannels = Resources.FindObjectsOfTypeAll<RippleStackTraceSO>();
            foreach (var ch in allChannels)
            {
                if (ch is not IChannel ich) continue;
                if (!_historyByInstanceId.TryGetValue(ch.GetInstanceID(), out var list))
                {
                    list = new List<int>();
                    _historyByInstanceId[ch.GetInstanceID()] = list;
                }
                list.Add(ich.SubscriberCount);
                if (list.Count >= SessionsToFlag && IsMonotonicallyIncreasing(list))
                {
                    Debug.LogWarning(
                        $"[Ripple] Possible listener leak on '{ch.name}' ({ch.GetType().Name}). " +
                        $"Subscriber count grew over {SessionsToFlag} sessions: {string.Join(",", list)}",
                        ch);
                }
                while (list.Count > SessionsToFlag) list.RemoveAt(0);
            }
        }

        private static bool IsMonotonicallyIncreasing(List<int> list)
        {
            for (int i = 1; i < list.Count; i++)
                if (list[i] <= list[i - 1]) return false;
            return true;
        }

        [MenuItem("Tools/Ripple/Reset Leak Detector History")]
        private static void Reset()
        {
            _historyByInstanceId.Clear();
            Debug.Log("[Ripple] Listener leak detector history cleared.");
        }
    }
}
