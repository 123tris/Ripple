using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Ripple
{
    internal static class Logger
    {
        private const int Capacity = 10000;

        private static readonly List<LogEntry> logs = new();

        public static IReadOnlyList<LogEntry> Logs => logs;

#if UNITY_EDITOR
        static Logger()
        {
            UnityEditor.EditorApplication.playModeStateChanged += state =>
            {
                if (state == UnityEditor.PlayModeStateChange.ExitingEditMode)
                    logs.Clear();
            };
        }
#endif

        public static void ResetLogs() => logs.Clear();

        [Conditional("UNITY_EDITOR")]
        public static void Log(UnityEngine.Object context, string caller, string value, string time)
        {
            if (logs.Count == Capacity)
                logs.RemoveAt(0);
            logs.Add(new LogEntry { Context = context, Asset = context.name, Caller = caller, Value = value, Time = time });
        }
    }

    [Serializable]
    internal class LogEntry
    {
        public UnityEngine.Object Context;
        public string Asset;
        public string Caller;
        public string Value;
        public string Time;
    }
}
