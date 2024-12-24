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
        static void OpenWindow()
        {
            var window = GetWindow<EventLoggerWindow>();
            window.name = "Ripple Event Logger";
            window.titleContent.text = window.name;
        }

        void OnEnable() => Logger.onLogAdded += _ => GetWindow<EventLoggerWindow>().Repaint();

        void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            foreach (LogEntry logEntry in Logger.GetLogs())
            {
                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button($"{logEntry.context.name}"))
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