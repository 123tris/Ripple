using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Ripple
{
    public class RippleDebugger : EditorWindow
    {
        private const string Log = "Log";

        [SerializeField] private VisualTreeAsset layout;
        [SerializeField] private string selected = Log;

        private readonly List<LogEntry> shown = new();
        private readonly Dictionary<string, int> ids = new();
        private MultiColumnListView log;
        private TreeView assets;
        private ScrollView inspector;
        private string filter = "";
        private LogEntry newest;

        [MenuItem("Tools/Ripple/Debugger")]
        private static void Open() => GetWindow<RippleDebugger>("Ripple Debugger").minSize = new Vector2(480, 200);

        private void OnEnable() => EditorApplication.projectChanged += ShowAssets;

        private void OnDisable() => EditorApplication.projectChanged -= ShowAssets;

        private void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            RippleStyle.Restore(this, ref layout);
            layout.CloneTree(root);

            log = root.Q<MultiColumnListView>("log");
            log.itemsSource = shown;
            Fill("time", entry => entry.time);
            Fill("asset", NameOf);
            Fill("caller", entry => entry.caller);
            Fill("value", entry => entry.value);
            log.selectionChanged += _ => EditorGUIUtility.PingObject((log.selectedItem as LogEntry)?.context);
            log.itemsChosen += chosen => Reveal((LogEntry)chosen.First());

            inspector = root.Q<ScrollView>("inspector");
            assets = root.Q<TreeView>("assets");
            assets.makeItem = MakeRow;
            assets.bindItem = (row, index) => BindRow(row, assets.GetItemDataForIndex<string>(index));
            assets.selectionChanged += _ => Select(assets.selectedItem as string);

            root.Q<ToolbarButton>("clear").clicked += Logger.ResetLogs;
            root.Q<ToolbarSearchField>("search").RegisterValueChangedCallback(change =>
            {
                filter = change.newValue;
                newest = null;
                ShowAssets();
            });

            ShowAssets();
            root.schedule.Execute(ShowLog).Every(250);
        }

        private void Fill(string column, Func<LogEntry, string> text)
        {
            log.columns[column].makeCell = () =>
            {
                var cell = new Label();
                cell.AddToClassList("ripple-cell");
                return cell;
            };
            log.columns[column].bindCell = (cell, row) => ((Label)cell).text = text(shown[row]);
        }

        private static VisualElement MakeRow()
        {
            var row = new VisualElement();
            row.AddToClassList("ripple-row");
            var icon = new Image();
            icon.AddToClassList("ripple-icon");
            row.Add(icon);
            row.Add(new Label());
            return row;
        }

        private void BindRow(VisualElement row, string item)
        {
            bool isAsset = ids.ContainsKey(item);
            var icon = row.Q<Image>();
            icon.image = isAsset ? AssetDatabase.GetCachedIcon(item) : item == Log ? EditorGUIUtility.IconContent("UnityEditor.ConsoleWindow").image : null;
            icon.EnableInClassList(RippleStyle.Hidden, !icon.image);
            row.Q<Label>().text = isAsset ? Path.GetFileNameWithoutExtension(item) : item;
        }

        private static string NameOf(LogEntry entry) => entry.context ? entry.context.name : "(destroyed)";

        private bool Matches(string name) => name.Contains(filter, StringComparison.OrdinalIgnoreCase);

        private bool Matches(LogEntry entry) => Matches(NameOf(entry)) || Matches(entry.caller);

        private void ShowLog()
        {
            LogEntry latest = Logger.Logs.Count == 0 ? null : Logger.Logs[Logger.Logs.Count - 1];
            if (latest == newest) return;

            newest = latest;
            shown.Clear();
            for (int i = Logger.Logs.Count - 1; i >= 0; i--)
                if (Matches(Logger.Logs[i]))
                    shown.Add(Logger.Logs[i]);
            log.RefreshItems();
        }

        private void ShowAssets()
        {
            if (assets == null) return;

            ids.Clear();
            var items = new List<TreeViewItemData<string>> { new(0, Log) };
            var groups = AssetDatabase.FindAssets("t:" + nameof(RippleStackTraceSO))
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => Matches(Path.GetFileNameWithoutExtension(path)))
                .GroupBy(path => ObjectNames.NicifyVariableName(AssetDatabase.GetMainAssetTypeAtPath(path).Name))
                .OrderBy(group => group.Key);
            foreach (IGrouping<string, string> group in groups)
            {
                int groupId = ids.Count + items.Count;
                var children = new List<TreeViewItemData<string>>();
                foreach (string path in group.OrderBy(Path.GetFileNameWithoutExtension))
                {
                    ids[path] = groupId + children.Count + 1;
                    children.Add(new TreeViewItemData<string>(ids[path], path));
                }

                items.Add(new TreeViewItemData<string>(groupId, group.Key, children));
            }

            assets.ClearSelection();
            assets.SetRootItems(items);
            assets.Rebuild();
            assets.ExpandAll();
            assets.SetSelectionById(ids.TryGetValue(selected, out int id) ? id : 0);
        }

        private void Select(string item)
        {
            if (item == null || item != Log && !ids.ContainsKey(item)) return;

            selected = item;
            UnityEngine.Object asset = item == Log ? null : AssetDatabase.LoadMainAssetAtPath(item);
            log.EnableInClassList(RippleStyle.Hidden, asset);
            inspector.EnableInClassList(RippleStyle.Hidden, !asset);
            inspector.Clear();
            if (asset)
                inspector.Add(new InspectorElement(asset));
        }

        private void Reveal(LogEntry entry)
        {
            if (ids.TryGetValue(AssetDatabase.GetAssetPath(entry.context), out int id))
                assets.SetSelectionById(id);
            else
                Selection.activeObject = entry.context;
        }
    }
}
