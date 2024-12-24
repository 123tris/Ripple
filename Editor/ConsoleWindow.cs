using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

public class LogViewerWindow : OdinEditorWindow
{
    [MenuItem("Window/Log Viewer")]
    public static void OpenWindow()
    {
        // Open the Odin editor window
        GetWindow<LogViewerWindow>().Show();
    }

    // This method is used to draw the log list with Odin
    [GUIColor(0f, 1f, 0f)] // You can set color for the UI
    private void DrawLogButtons()
    {
        if (LogManager.Logs.Count == 0)
        {
            GUILayout.Label("No logs available.");
            return;
        }

        // Loop through all logs and display them as clickable buttons
        foreach (var log in LogManager.Logs)
        {
            // Draw a button for each log entry
            if (GUILayout.Button($"Log: {log.Message}"))
            {
                // When clicked, select the corresponding object in the Unity Editor
                Selection.activeObject = log.Object;
            }
        }
    }

    // Override the DrawEditor() method to render the GUI
    protected override void DrawEditor(int index)
    {
        base.DrawEditor(index);
        // Title for the log viewer
        GUILayout.Label("Log Viewer", EditorStyles.boldLabel);

        // Call the method to draw the logs
        DrawLogButtons();
    }

    [Button]
    void Log(string message, UnityEngine.Object obj)
    {
        LogManager.Log(message,obj);
    }

}

public static class LogManager
{
    public static List<LogEntry> Logs = new List<LogEntry>();

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
