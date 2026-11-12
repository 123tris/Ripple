using System;
using System.Collections.Generic;
using System.Diagnostics;
using Sirenix.OdinInspector;

namespace Ripple
{
    public static class Logger
    {
        private static readonly List<LogEntry> logs = new();

        public static Action<LogEntry> onLogAdded;

        public static List<LogEntry> GetLogs() => new(logs);

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
        public static void Log(string message, UnityEngine.Object context)
        {
            LogEntry logEntry = new() { message = message, context = context };
            logs.Add(logEntry);
            onLogAdded?.Invoke(logEntry);
        }
    }

    public class LogEntry
    {
        [HorizontalGroup("Log Entry"), HideLabel]
        public UnityEngine.Object context;

        [HorizontalGroup("Log Entry"), HideLabel]
        public string message;
    }
}
