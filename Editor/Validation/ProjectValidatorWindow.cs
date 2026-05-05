using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Ripple.Editor
{
    public sealed class ProjectValidatorWindow : EditorWindow
    {
        [MenuItem("Tools/Ripple/Validate Project")]
        public static void Open() => GetWindow<ProjectValidatorWindow>("Ripple Validator").Show();

        private List<string> _missingRefs = new();
        private List<string> _orphanAssets = new();
        private Vector2 _scroll;

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            if (GUILayout.Button("Run scan", EditorStyles.toolbarButton, GUILayout.Width(80))) Scan();
            EditorGUILayout.EndHorizontal();

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            EditorGUILayout.LabelField($"Missing references ({_missingRefs.Count})", EditorStyles.boldLabel);
            foreach (var line in _missingRefs) EditorGUILayout.LabelField(line);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Orphan Ripple assets ({_orphanAssets.Count})", EditorStyles.boldLabel);
            foreach (var line in _orphanAssets) EditorGUILayout.LabelField(line);

            EditorGUILayout.EndScrollView();
        }

        private void Scan()
        {
            _missingRefs.Clear();
            _orphanAssets.Clear();

            var allRipple = Resources.FindObjectsOfTypeAll<RippleStackTraceSO>();
            ReferenceUsageIndex.Rebuild();

            foreach (var so in allRipple)
            {
                var path = AssetDatabase.GetAssetPath(so);
                if (string.IsNullOrEmpty(path)) continue;
                var usages = ReferenceUsageIndex.FindUsages(so);
                if (usages.Count == 0) _orphanAssets.Add($"{so.name} ({so.GetType().Name}) at {path}");
            }

            var monoGuids = AssetDatabase.FindAssets("t:Prefab t:SceneAsset");
            foreach (var guid in monoGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var deps = AssetDatabase.GetDependencies(path, false);
                foreach (var dep in deps)
                {
                    var loaded = AssetDatabase.LoadMainAssetAtPath(dep);
                    if (loaded == null) _missingRefs.Add($"Missing dependency referenced by {path}: {dep}");
                }
            }
        }
    }
}
