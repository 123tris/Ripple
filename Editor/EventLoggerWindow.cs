using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Ripple
{
    public class EventLoggerWindow : EditorWindow
    {
        private Vector2 scrollPosition;

        [MenuItem("Tools/Open Ripple Event Logger")]
        static void OpenWindow() => GetWindow<EventLoggerWindow>("Ripple Event Logger");

        void OnEnable() => Logger.onLogAdded += _ => Repaint();

        void OnGUI()
        {
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

                    var labelStyle = EditorStyles.label;
                    labelStyle.richText = true;
                    GUILayout.Label(logEntry.message, labelStyle);
                }
                GUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }
    }
}