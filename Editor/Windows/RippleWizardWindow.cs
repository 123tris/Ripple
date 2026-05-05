using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Ripple.Editor
{
    public sealed class RippleWizardWindow : EditorWindow
    {
        [MenuItem("Tools/Ripple/Wizard")]
        public static void Open()
        {
            GetWindow<RippleWizardWindow>("Ripple Wizard").Show();
        }

        private Vector2 _scroll;
        private string _filter = string.Empty;
        private readonly Dictionary<Type, bool> _expanded = new();

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            _filter = GUILayout.TextField(_filter, EditorStyles.toolbarSearchField);
            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(70)))
                AssetCache.Invalidate();
            EditorGUILayout.EndHorizontal();

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            var groups = RippleTypeCache.Grouped().OrderBy(g => g.Key);
            foreach (var group in groups)
            {
                EditorGUILayout.LabelField(group.Key, EditorStyles.boldLabel);
                foreach (var type in group.OrderBy(t => t.Name))
                {
                    if (!_expanded.TryGetValue(type, out var open)) open = false;
                    var assets = AssetCache.Find(type);
                    var filtered = string.IsNullOrEmpty(_filter)
                        ? assets
                        : assets.Where(a => a.name.IndexOf(_filter, StringComparison.OrdinalIgnoreCase) >= 0).ToArray();
                    if (!string.IsNullOrEmpty(_filter) && filtered.Length == 0) continue;

                    EditorGUI.indentLevel++;
                    var label = $"{type.Name} ({filtered.Length})";
                    open = EditorGUILayout.Foldout(open, label, true);
                    _expanded[type] = open;
                    if (open)
                    {
                        EditorGUI.indentLevel++;
                        foreach (var asset in filtered)
                        {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.ObjectField(asset, type, false);
                            if (GUILayout.Button("Ping", GUILayout.Width(50)))
                                EditorGUIUtility.PingObject(asset);
                            EditorGUILayout.EndHorizontal();
                        }
                        EditorGUI.indentLevel--;
                    }
                    EditorGUI.indentLevel--;
                }
            }

            EditorGUILayout.EndScrollView();
        }
    }
}
