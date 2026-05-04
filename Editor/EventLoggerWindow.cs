using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Ripple
{
    public class EventLoggerWindow : EditorWindow
    {
        private Vector2 scrollPosition;
        private readonly HashSet<int> _expandedEntries = new();
        private readonly Dictionary<int, Vector2> _stackScrollPositions = new();
        private int _selectedTab = 0;
        private string _searchFilter = "";
        private LogSourceKind? _sourceFilter = null;

        [MenuItem("Tools/Open Ripple Event Logger")]
        static void OpenWindow() => GetWindow<EventLoggerWindow>("Ripple Event Logger");

        void OnEnable()
        {
            Logger.onLogAdded += OnLogAdded;
            _searchFilter = SessionState.GetString("RippleLogger_SearchFilter", "");
            int sourceIndex = SessionState.GetInt("RippleLogger_SourceFilter", -1);
            _sourceFilter = sourceIndex >= 0 ? (LogSourceKind?)sourceIndex : null;
        }

        void OnDisable()
        {
            Logger.onLogAdded -= OnLogAdded;
            SessionState.SetString("RippleLogger_SearchFilter", _searchFilter);
            SessionState.SetInt("RippleLogger_SourceFilter", _sourceFilter.HasValue ? (int)_sourceFilter.Value : -1);
        }

        private void OnLogAdded(LogEntry _)
        {
            Repaint();
        }

        void OnGUI()
        {
            DrawToolbar();
            _selectedTab = GUILayout.Toolbar(_selectedTab, new string[] { "Ripple Logs", "General Logs" });
            DrawFilters();
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            if (_selectedTab == 0)
            {
                List<LogEntry> logs = Logger.GetLogs();
                DrawRippleLogs(logs);
            }
            else
            {
                DrawGeneralLogs();
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawToolbar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("Logs", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Clear", EditorStyles.toolbarButton))
            {
                if (_selectedTab == 0)
                    Logger.ResetLogs();
                else
                    GeneralLogManager.Logs.Clear();
                _expandedEntries.Clear();
            }

            GUILayout.EndHorizontal();
        }

        private void DrawFilters()
        {
            GUILayout.BeginHorizontal();
            _searchFilter = EditorGUILayout.TextField("Search", _searchFilter);

            string[] sourceOptions = { "All", nameof(LogSourceKind.General), nameof(LogSourceKind.EventInvoke), nameof(LogSourceKind.VariableChange) };
            int currentSourceIndex = _sourceFilter.HasValue ? ((int)_sourceFilter.Value + 1) : 0;
            int selectedSourceIndex = EditorGUILayout.Popup("Source", currentSourceIndex, sourceOptions);
            _sourceFilter = selectedSourceIndex == 0 ? null : (LogSourceKind?)(selectedSourceIndex - 1);

            if (GUILayout.Button("Clear Filters"))
            {
                _searchFilter = "";
                _sourceFilter = null;
            }

            GUILayout.EndHorizontal();
        }

        private void DrawRippleLogs(List<LogEntry> logs)
        {
            for (int index = 0; index < logs.Count; index++)
            {
                var log = logs[index];
                if (!MatchesFilter(log)) continue;
                DrawEntry(index, log);
            }
        }

        private void DrawGeneralLogs()
        {
            for (int index = 0; index < GeneralLogManager.Logs.Count; index++)
            {
                var log = GeneralLogManager.Logs[index];
                if (!string.IsNullOrEmpty(_searchFilter) && !log.Message.Contains(_searchFilter)) continue;
                DrawGeneralEntry(log);
            }
        }

        private bool MatchesFilter(LogEntry log)
        {
            if (!string.IsNullOrEmpty(_searchFilter) && !log.message.Contains(_searchFilter) &&
                !log.caller.Contains(_searchFilter)) return false;
            if (_sourceFilter.HasValue && log.sourceKind != _sourceFilter.Value) return false;
            return true;
        }

        private void DrawEntry(int index, LogEntry logEntry)
        {
            Color previousColor = GUI.color;
            if (logEntry.hasException)
                GUI.color = new Color(1f, 0.75f, 0.75f);
            else if (logEntry.sourceKind == LogSourceKind.VariableChange)
                GUI.color = new Color(0.8f, 0.95f, 1f);
            else if (logEntry.sourceKind == LogSourceKind.EventInvoke)
                GUI.color = new Color(0.9f, 1f, 0.85f);

            GUILayout.BeginVertical("box");
            GUI.color = previousColor;

            GUILayout.BeginHorizontal();
            DrawContextButton(logEntry);
            GUILayout.FlexibleSpace();

            string timestamp = logEntry.timestamp.ToString("HH:mm:ss.fff");
            GUILayout.Label(timestamp, EditorStyles.miniLabel, GUILayout.Width(70));

            bool isExpanded = _expandedEntries.Contains(index);
            if (GUILayout.Button(isExpanded ? "Hide Stack" : "Show Stack", GUILayout.Width(90)))
            {
                if (isExpanded)
                    _expandedEntries.Remove(index);
                else
                    _expandedEntries.Add(index);
            }

            if (GUILayout.Button("Copy", GUILayout.Width(50)))
                GUIUtility.systemCopyBuffer = BuildClipboardText(logEntry);

            GUILayout.EndHorizontal();

            var metadataStyle = new GUIStyle(EditorStyles.miniBoldLabel) { wordWrap = true };
            GUILayout.Label(BuildMetadataLabel(logEntry), metadataStyle);

            var labelStyle = new GUIStyle(EditorStyles.label) { richText = true, wordWrap = true };
            GUILayout.Label(logEntry.message, labelStyle);

            if (isExpanded && !string.IsNullOrWhiteSpace(logEntry.fullStackTrace))
            {
                DrawStackTrace(index, logEntry.fullStackTrace);
            }

            if (logEntry.hasException && !string.IsNullOrWhiteSpace(logEntry.exceptionDetails))
            {
                EditorGUILayout.HelpBox(logEntry.exceptionDetails, MessageType.Error);
            }

            GUILayout.EndVertical();
        }

        private static string BuildMetadataLabel(LogEntry entry)
        {
            string frameText = entry.frame >= 0 ? $"Frame {entry.frame}" : "Editor";
            string callerText = string.IsNullOrWhiteSpace(entry.caller) ? "Caller unknown" : entry.caller;
            string invokerText = string.IsNullOrWhiteSpace(entry.invokerName) ? "Invoker unknown" : entry.invokerName;
            string statusText = entry.hasException ? "EXCEPTION" : "OK";
            return $"[{entry.sourceKind}] [{statusText}] {frameText} #{entry.sequenceInFrame} | {invokerText} | {callerText}";
        }

        private static string BuildClipboardText(LogEntry entry)
        {
            return $"[{entry.sourceKind}] {entry.timestamp:HH:mm:ss.fff} Frame {entry.frame} #{entry.sequenceInFrame}\n" +
                   $"Context: {(entry.context != null ? entry.context.name : "none")}\n" +
                   $"Invoker: {entry.invokerName}\nCaller: {entry.caller}\n" +
                   $"Message: {entry.message}\n" +
                   (entry.hasException ? $"Exception: {entry.exceptionDetails}\n" : "") +
                   (string.IsNullOrWhiteSpace(entry.fullStackTrace) ? "" : $"Stack:\n{entry.fullStackTrace}");
        }

        private static void DrawContextButton(LogEntry logEntry)
        {
            string contextName = logEntry.context != null ? logEntry.context.name : "(no context)";
            var size = GUI.skin.button.CalcSize(new GUIContent(contextName));
            size.x = Mathf.Clamp(size.x, 140f, 220f);
            if (!GUILayout.Button(contextName, GUILayout.Width(size.x), GUILayout.Height(size.y)))
                return;

            if (logEntry.context == null)
                return;

            Selection.activeObject = logEntry.context;
            EditorApplication.ExecuteMenuItem("Window/General/Project");
            EditorGUIUtility.PingObject(logEntry.context);
        }

        private void DrawStackTrace(int index, string fullStackTrace)
        {
            if (!_stackScrollPositions.TryGetValue(index, out var stackScroll))
                stackScroll = Vector2.zero;

            var background = new GUIStyle("box");
            GUILayout.BeginVertical(background);
            GUILayout.Label("Call stack", EditorStyles.miniBoldLabel);

            stackScroll = EditorGUILayout.BeginScrollView(
                stackScroll,
                false,
                true,
                GUILayout.Height(140),
                GUILayout.ExpandWidth(true));

            var stackStyle = new GUIStyle(EditorStyles.textArea)
            {
                wordWrap = true,
                richText = false
            };
            EditorGUILayout.SelectableLabel(
                fullStackTrace,
                stackStyle,
                GUILayout.ExpandWidth(true),
                GUILayout.ExpandHeight(true));

            EditorGUILayout.EndScrollView();
            GUILayout.EndVertical();
            _stackScrollPositions[index] = stackScroll;
        }

        private void DrawGeneralEntry(GeneralLogManager.LogEntry logEntry)
        {
            GUILayout.BeginVertical("box");

            GUILayout.BeginHorizontal();
            string contextName = logEntry.Object != null ? logEntry.Object.name : "(no context)";
            var size = GUI.skin.button.CalcSize(new GUIContent(contextName));
            size.x = Mathf.Clamp(size.x, 140f, 220f);
            if (GUILayout.Button(contextName, GUILayout.Width(size.x), GUILayout.Height(size.y)))
            {
                if (logEntry.Object != null)
                {
                    Selection.activeObject = logEntry.Object;
                    EditorApplication.ExecuteMenuItem("Window/General/Project");
                    EditorGUIUtility.PingObject(logEntry.Object);
                }
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            var labelStyle = new GUIStyle(EditorStyles.label) { richText = true, wordWrap = true };
            GUILayout.Label(logEntry.Message, labelStyle);

            GUILayout.EndVertical();
        }

        private static class GeneralLogManager
        {
            public static readonly List<LogEntry> Logs = new();

            public static void Log(string message, UnityEngine.Object obj)
            {
                Logs.Add(new LogEntry(message, obj));
            }

            public class LogEntry
            {
                public string Message;
                public UnityEngine.Object Object;

                public LogEntry(string message, UnityEngine.Object obj)
                {
                    Message = message;
                    Object = obj;
                }
            }
        }
    }
}
