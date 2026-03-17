using System;
using UnityEditor;
using UnityEngine;

namespace Ripple
{
    public class EventLoggerWindow : EditorWindow
    {
        private Vector2 scrollPosition;

        [MenuItem("Tools/Ripple/Open Event Logger")]
        static void OpenWindow() => GetWindow<EventLoggerWindow>("Ripple Event Logger");

        void OnEnable() => Logger.onLogAdded += _ => Repaint();

        void OnGUI()
        {
            // Top toolbar
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                if (GUILayout.Button("Clear Logs", EditorStyles.toolbarButton))
                {
                    Logger.ResetLogs();
                }

                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();

            // Scroll view
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            var logs = Logger.GetLogs();

            for (int i = logs.Count - 1; i >= 0; i--)
            {
                LogEntry logEntry = logs[i];

                GUILayout.BeginHorizontal();
                {
                    var size = GUI.skin.button.CalcSize(new GUIContent(logEntry.context.name));
                    size.x = Math.Max(100, size.x);

                    if (GUILayout.Button($"{logEntry.context.name}", GUILayout.Width(size.x), GUILayout.Height(size.y)))
                    {
                        Selection.activeObject = logEntry.context;
                        EditorApplication.ExecuteMenuItem("Window/General/Project");
                        EditorGUIUtility.PingObject(logEntry.context);
                    }

                    var labelStyle = new GUIStyle(EditorStyles.label); // avoid modifying shared style
                    labelStyle.richText = true;

                    GUILayout.Label(logEntry.message, labelStyle);
                }
                GUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }
    }
}