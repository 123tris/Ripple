using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Ripple
{
    internal static class StackTraceUtility
    {
        public static StackTrace BuildStackTrace(int skipFrames)
        {
            return new StackTrace(skipFrames, true);
        }

        public static string ResolveCaller(StackTrace stackTrace, int startFrame)
        {
            if (stackTrace == null || stackTrace.FrameCount == 0)
                return string.Empty;

            for (int frameIndex = startFrame; frameIndex < stackTrace.FrameCount; frameIndex++)
            {
                var frame = stackTrace.GetFrame(frameIndex);
                var method = frame?.GetMethod();
                var declaringType = method?.DeclaringType;
                if (method == null || declaringType == null)
                    continue;

                var declaringNamespace = declaringType.Namespace ?? string.Empty;
                if (declaringNamespace.StartsWith("Ripple", StringComparison.Ordinal))
                    continue;

                return $"{declaringType.FullName}->{method.Name}";
            }

            var fallbackMethod = stackTrace.GetFrame(startFrame)?.GetMethod() ?? stackTrace.GetFrame(0)?.GetMethod();
            if (fallbackMethod?.DeclaringType == null)
                return string.Empty;
            return $"{fallbackMethod.DeclaringType.FullName}->{fallbackMethod.Name}";
        }

        public static string FormatStackTrace(StackTrace stackTrace, int startFrame)
        {
            if (stackTrace == null)
                return string.Empty;

            var lines = new List<string>();
            for (int frameIndex = startFrame; frameIndex < stackTrace.FrameCount; frameIndex++)
            {
                var frame = stackTrace.GetFrame(frameIndex);
                var method = frame?.GetMethod();
                if (method == null)
                    continue;

                var typeName = method.DeclaringType?.FullName ?? "<unknown-type>";
                var methodName = method.Name;
                var fileName = frame.GetFileName();
                var lineNumber = frame.GetFileLineNumber();

                if (!string.IsNullOrWhiteSpace(fileName) && lineNumber > 0)
                    lines.Add($"{frameIndex - startFrame:00}: {typeName}->{methodName} ({System.IO.Path.GetFileName(fileName)}:{lineNumber})");
                else
                    lines.Add($"{frameIndex - startFrame:00}: {typeName}->{methodName}");
            }

            return string.Join("\n", lines);
        }
    }
}
