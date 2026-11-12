using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Ripple
{
    public class EventLoggerWindow : EditorWindow
    {
        private Vector2 scrollPosition;
        private GUIStyle messageStyle;

        [MenuItem("Tools/Ripple/Event Logger")]
        private static void OpenWindow() => GetWindow<EventLoggerWindow>("Ripple Event Logger");

        private void OnEnable() => Logger.onLogAdded += OnLogAdded;

        private void OnDisable() => Logger.onLogAdded -= OnLogAdded;

        private void OnLogAdded(LogEntry entry) => Repaint();

        private void OnGUI()
        {
            messageStyle ??= new GUIStyle(EditorStyles.label) { richText = true };

            using (new GUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                if (GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
                    Logger.ResetLogs();
                GUILayout.FlexibleSpace();
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            List<LogEntry> logs = Logger.GetLogs();
            for (int i = logs.Count - 1; i >= 0; i--)
                DrawEntry(logs[i]);
            EditorGUILayout.EndScrollView();
        }

        private void DrawEntry(LogEntry entry)
        {
            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button(entry.context.name, GUILayout.MinWidth(100), GUILayout.ExpandWidth(false)))
                {
                    Selection.activeObject = entry.context;
                    EditorUtility.FocusProjectWindow();
                    EditorGUIUtility.PingObject(entry.context);
                }

                GUILayout.Label(entry.message, messageStyle);
            }
        }
    }
}
