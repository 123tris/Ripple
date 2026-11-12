using System.Collections.Generic;
using System.Diagnostics;

namespace Ripple
{
    public static class Logger
    {
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
        public static void Log(UnityEngine.Object context, string caller, string value, string time) =>
            logs.Add(new LogEntry { context = context, caller = caller, value = value, time = time });
    }

    public class LogEntry
    {
        public UnityEngine.Object context;
        public string caller;
        public string value;
        public string time;
    }
}
