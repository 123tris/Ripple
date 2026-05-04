using System;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Ripple
{
    public abstract class RippleStackTraceSO : ScriptableObject
    {
#if UNITY_EDITOR
        [SerializeField, TextArea, HideInInlineEditors]
        private string _developerNotes;

        [ShowInInspector, ShowIf(nameof(HasTraceHistory)), BoxGroup("Debug", order: 1), HideInInlineEditors,
         LabelText("Call stack history"),
         ListDrawerSettings(ShowFoldout = true, DraggableItems = false, HideAddButton = true, HideRemoveButton = true)]
        private System.Collections.Generic.IReadOnlyList<LogEntry> TraceHistory
        {
            get
            {
                var contextLogs = Logger.GetLogsForContext(this);
                return contextLogs.Skip(Math.Max(0, contextLogs.Count - 25)).ToList();
            }
        }

        private bool HasTraceHistory => Logger.GetLogsForContext(this).Count > 0;

#endif

        protected void LogInvoke<T>(T parameter)
        {
            LogInvoke(parameter, null);
        }

        protected void LogInvoke<T>(T parameter, UnityEngine.Object invoker)
        {
#if UNITY_EDITOR
            const int baseSkipFrames = 3;
            var stackTrace = StackTraceUtility.BuildStackTrace(baseSkipFrames);
            var caller = StackTraceUtility.ResolveCaller(stackTrace, baseSkipFrames);
            var fullStackTrace = StackTraceUtility.FormatStackTrace(stackTrace, baseSkipFrames);

            Logger.Log(new LogMetadata
            {
                Message = $"Called by: <color=red>{caller}</color> \nWith value: <color=green>{parameter}</color>",
                Context = this,
                Invoker = invoker,
                Caller = caller,
                FullStackTrace = fullStackTrace,
                HasException = false,
                ExceptionDetails = string.Empty,
                SourceKind = this is BaseVariable ? LogSourceKind.VariableChange : LogSourceKind.EventInvoke
            });
#endif
        }

        protected void LogInvoke()
        {
            LogInvoke(null);
        }

        protected void LogInvoke(UnityEngine.Object invoker)
        {
#if UNITY_EDITOR
            const int baseSkipFrames = 3;
            var stackTrace = StackTraceUtility.BuildStackTrace(baseSkipFrames);
            var caller = StackTraceUtility.ResolveCaller(stackTrace, baseSkipFrames);
            var fullStackTrace = StackTraceUtility.FormatStackTrace(stackTrace, baseSkipFrames);

            Logger.Log(new LogMetadata
            {
                Message = $"Called by: <color=red>{caller}</color>",
                Context = this,
                Invoker = invoker,
                Caller = caller,
                FullStackTrace = fullStackTrace,
                HasException = false,
                ExceptionDetails = string.Empty,
                SourceKind = LogSourceKind.EventInvoke
            });
#endif
        }
    }
}