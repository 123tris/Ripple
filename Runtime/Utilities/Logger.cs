using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ripple
{
    public static class Logger
    {
        private static readonly List<LogEntry> logs = new();
        private static int _lastLoggedFrame = -1;
        private static int _frameSequenceIndex = -1;

        public static Action<LogEntry> onLogAdded;
        
        public static List<LogEntry> GetLogs() => new(logs);

        public static List<LogEntry> GetLogsForContext(UnityEngine.Object context) => logs.Where(l => l.context == context).ToList();

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

        public static void ResetLogs()
        {
            logs.Clear();
            _lastLoggedFrame = -1;
            _frameSequenceIndex = -1;
        }

        public static void Log(string message, UnityEngine.Object context)
        {
#if UNITY_EDITOR
            Log(new LogMetadata
            {
                Message = message,
                Context = context,
                Invoker = null,
                Caller = string.Empty,
                FullStackTrace = string.Empty,
                HasException = false,
                ExceptionDetails = string.Empty,
                SourceKind = LogSourceKind.General
            });
#endif
        }

        public static void Log(LogMetadata metadata)
        {
#if UNITY_EDITOR
            int frameCount = Application.isPlaying ? Time.frameCount : -1;
            if (frameCount != _lastLoggedFrame)
            {
                _lastLoggedFrame = frameCount;
                _frameSequenceIndex = 0;
            }
            else
            {
                _frameSequenceIndex++;
            }

            LogEntry logEntry = new()
            {
                message = metadata.Message,
                context = metadata.Context,
                invokerName = metadata.Invoker != null ? metadata.Invoker.name : string.Empty,
                caller = metadata.Caller,
                fullStackTrace = metadata.FullStackTrace,
                hasException = metadata.HasException,
                exceptionDetails = metadata.ExceptionDetails,
                sourceKind = metadata.SourceKind,
                frame = frameCount,
                sequenceInFrame = _frameSequenceIndex,
                timestamp = DateTime.Now
            };

            logs.Add(logEntry);
            onLogAdded?.Invoke(logEntry);
#endif
        }
    }

    public enum LogSourceKind
    {
        General,
        EventInvoke,
        VariableChange
    }

    public struct LogMetadata
    {
        public string Message;
        public UnityEngine.Object Context;
        public UnityEngine.Object Invoker;
        public string Caller;
        public string FullStackTrace;
        public bool HasException;
        public string ExceptionDetails;
        public LogSourceKind SourceKind;
    }

    public class LogEntry
    {
        public UnityEngine.Object context;

        public string message;
        public string invokerName;
        public string caller;
        public string fullStackTrace;
        public bool hasException;
        public string exceptionDetails;
        public LogSourceKind sourceKind;
        public int frame;
        public int sequenceInFrame;
        public DateTime timestamp;
    }
}