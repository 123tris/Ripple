using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Ripple
{
    public class RippleDebugger : OdinMenuEditorWindow
    {
        private static string[] assetGuids;

        private GUIStyle messageStyle;

        private bool ShowsLog => MenuTree.Selection.SelectedValue == null;

        [MenuItem("Tools/Ripple/Debugger")]
        private static void OpenWindow() => GetWindow<RippleDebugger>("Ripple Debugger");

        protected override void OnEnable()
        {
            base.OnEnable();
            Logger.onLogAdded += OnLogAdded;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            Logger.onLogAdded -= OnLogAdded;
        }

        private void OnLogAdded(LogEntry entry) => Repaint();

        protected override OdinMenuTree BuildMenuTree()
        {
            var tree = new OdinMenuTree { DefaultMenuStyle = OdinMenuStyle.TreeViewStyle };
            tree.Config.DrawSearchToolbar = true;
            tree.Add("Log", null);

            var types = new HashSet<Type>(TypeCache.GetTypesWithAttribute<RippleData>());
            assetGuids ??= FindAssets(types);

            foreach (string guid in assetGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Type type = AssetDatabase.GetMainAssetTypeAtPath(path);
                if (types.Contains(type))
                    tree.AddAssetAtPath(type.Name + "/" + Path.GetFileNameWithoutExtension(path), path);
            }

            tree.EnumerateTree().AddThumbnailIcons(true);
            tree.MenuItems[0].Select();
            return tree;
        }

        private static string[] FindAssets(IEnumerable<Type> types)
        {
            string filter = "t:" + string.Join(" t:", types.Select(type => type.Name));
            if (filter.Length > 1000)
                filter = "t:ScriptableObject";
            return AssetDatabase.FindAssets(filter, new[] { "Assets" });
        }

        protected override void OnBeginDrawEditors()
        {
            SirenixEditorGUI.BeginHorizontalToolbar(MenuTree.Config.SearchToolbarHeight);
            GUILayout.Label(ShowsLog ? "Log" : "Assets: " + assetGuids.Length);
            GUILayout.FlexibleSpace();

            if (ShowsLog && SirenixEditorGUI.ToolbarButton("Clear"))
                Logger.ResetLogs();

            if (SirenixEditorGUI.ToolbarButton(new GUIContent(EditorIcons.Refresh.Raw, "Refresh the asset list")))
            {
                assetGuids = null;
                ForceMenuTreeRebuild();
            }

            SirenixEditorGUI.EndHorizontalToolbar();
        }

        protected override void DrawEditors()
        {
            if (!ShowsLog)
            {
                base.DrawEditors();
                return;
            }

            messageStyle ??= new GUIStyle(EditorStyles.label) { richText = true };
            string filter = MenuTree.Config.SearchTerm;
            List<LogEntry> logs = Logger.GetLogs();
            for (int i = logs.Count - 1; i >= 0; i--)
                if (string.IsNullOrEmpty(filter) || NameOf(logs[i]).Contains(filter, StringComparison.OrdinalIgnoreCase))
                    DrawEntry(logs[i]);
        }

        private void DrawEntry(LogEntry entry)
        {
            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button(NameOf(entry), GUILayout.MinWidth(100), GUILayout.ExpandWidth(false)))
                    Show(entry.context);

                GUILayout.Label(entry.message, messageStyle);
            }
        }

        private static string NameOf(LogEntry entry) => entry.context ? entry.context.name : "(destroyed)";

        private void Show(UnityEngine.Object asset)
        {
            OdinMenuItem item = MenuTree.EnumerateTree().FirstOrDefault(candidate => ReferenceEquals(candidate.Value, asset));
            if (item != null)
                item.Select();
            else
                Selection.activeObject = asset;
        }
    }
}
