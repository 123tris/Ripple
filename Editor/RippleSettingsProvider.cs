using Ripple;
using UnityEditor;
using UnityEngine;

namespace Ripple.Editor
{
    internal static class RippleSettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateProvider()
        {
            return new SettingsProvider("Project/Ripple", SettingsScope.Project)
            {
                label = "Ripple",
                guiHandler = _ => DrawGUI(),
                keywords = new[] { "Ripple", "Events", "Variables", "ScriptableObject" }
            };
        }

        private static void DrawGUI()
        {
            var settings = RippleSettings.Instance;
            if (settings == null)
            {
                EditorGUILayout.HelpBox("No RippleSettings asset found.", MessageType.Warning);
                if (GUILayout.Button("Create RippleSettings"))
                    CreateAndAssignSettings();
                return;
            }

            var so = new SerializedObject(settings);

            EditorGUILayout.LabelField("Event Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(so.FindProperty("_defaultClearListenersOnPlaymode"),
                new GUIContent("Clear Listeners On Playmode", "Default value for all new GameEvent assets."));

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Trace / Debug Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(so.FindProperty("_maxTraceHistoryCount"),
                new GUIContent("Max Trace History", "Maximum call history entries stored per event/variable."));
            EditorGUILayout.PropertyField(so.FindProperty("_enableStackTraceCapture"),
                new GUIContent("Enable Stack Trace Capture", "Disable for better editor performance in large projects."));

            so.ApplyModifiedProperties();
        }

        private static void CreateAndAssignSettings()
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Create RippleSettings", "RippleSettings", "asset", "Choose location for RippleSettings asset");
            if (string.IsNullOrEmpty(path)) return;

            var settings = ScriptableObject.CreateInstance<RippleSettings>();
            AssetDatabase.CreateAsset(settings, path);
            AssetDatabase.SaveAssets();
            RippleSettings.SetInstance(settings);
        }
    }
}
