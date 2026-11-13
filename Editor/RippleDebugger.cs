using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Properties;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Ripple
{
    public class RippleDebugger : EditorWindow
    {
        private const string AssetView = "ripple-debugger--asset";

        [SerializeField] private VisualTreeAsset layout;
        [SerializeField] private string selected = "";

        private readonly Dictionary<string, int> ids = new();
        private VisualElement root;
        private MultiColumnListView log;
        private TreeView assets;
        private ScrollView inspector;
        private string filter = "";
        private LogEntry newest;

        [CreateProperty] internal List<LogEntry> Log { get; } = new();

        internal sealed class Entry
        {
            public string Name;
            public Texture Icon;
            public string AssetPath;
        }

        [MenuItem("Tools/Ripple/Debugger")]
        private static void Open() => GetWindow<RippleDebugger>("Ripple Debugger").minSize = new Vector2(480, 200);

        private void OnEnable() => EditorApplication.projectChanged += ShowAssets;

        private void OnDisable() => EditorApplication.projectChanged -= ShowAssets;

        private void CreateGUI()
        {
            RippleAssets.Restore(this, ref layout);
            layout.CloneTree(rootVisualElement);
            rootVisualElement.dataSource = this;
            root = rootVisualElement.Q(className: "ripple-debugger");

            log = root.Q<MultiColumnListView>("log");
            log.selectionChanged += _ => EditorGUIUtility.PingObject((log.selectedItem as LogEntry)?.Context);
            log.itemsChosen += chosen => Reveal((LogEntry)chosen.First());

            inspector = root.Q<ScrollView>("inspector");
            assets = root.Q<TreeView>("assets");
            assets.bindItem = (row, index) => row.dataSource = assets.GetItemDataForIndex<Entry>(index);
            assets.selectionChanged += _ => Select(assets.selectedItem as Entry);

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

        private static Texture Icon(string name) => EditorGUIUtility.IconContent(name).image;

        private bool Matches(string name) => name.Contains(filter, StringComparison.OrdinalIgnoreCase);

        private bool Matches(LogEntry entry) => Matches(entry.Asset) || Matches(entry.Caller);

        private void ShowLog()
        {
            LogEntry latest = Logger.Logs.Count == 0 ? null : Logger.Logs[Logger.Logs.Count - 1];
            if (latest == newest) return;

            newest = latest;
            Log.Clear();
            for (int i = Logger.Logs.Count - 1; i >= 0; i--)
                if (Matches(Logger.Logs[i]))
                    Log.Add(Logger.Logs[i]);
            log.RefreshItems();
        }

        private void ShowAssets()
        {
            if (assets == null) return;

            ids.Clear();
            ids[""] = 0;
            int id = 1;
            var items = new List<TreeViewItemData<Entry>> { new(0, new Entry { Name = "Log", Icon = Icon("UnityEditor.ConsoleWindow"), AssetPath = "" }) };
            var groups = AssetDatabase.FindAssets("t:" + nameof(RippleStackTraceSO))
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => Matches(Path.GetFileNameWithoutExtension(path)))
                .GroupBy(path => ObjectNames.NicifyVariableName(AssetDatabase.GetMainAssetTypeAtPath(path).Name))
                .OrderBy(group => group.Key);
            foreach (IGrouping<string, string> group in groups)
            {
                int groupId = id++;
                var children = new List<TreeViewItemData<Entry>>();
                foreach (string path in group.OrderBy(Path.GetFileNameWithoutExtension))
                {
                    ids[path] = id;
                    children.Add(new TreeViewItemData<Entry>(id++, new Entry { Name = Path.GetFileNameWithoutExtension(path), Icon = AssetDatabase.GetCachedIcon(path), AssetPath = path }));
                }

                items.Add(new TreeViewItemData<Entry>(groupId, new Entry { Name = group.Key, Icon = Icon("Folder Icon") }, children));
            }

            assets.ClearSelection();
            assets.SetRootItems(items);
            assets.Rebuild();
            assets.ExpandAll();
            assets.SetSelectionById(ids.TryGetValue(selected, out int found) ? found : 0);
        }

        private void Select(Entry entry)
        {
            if (entry?.AssetPath == null) return;

            selected = entry.AssetPath;
            UnityEngine.Object asset = entry.AssetPath == "" ? null : AssetDatabase.LoadMainAssetAtPath(entry.AssetPath);
            root.EnableInClassList(AssetView, asset);
            inspector.Clear();
            if (asset)
                inspector.Add(new InspectorElement(asset));
        }

        private void Reveal(LogEntry entry)
        {
            if (ids.TryGetValue(AssetDatabase.GetAssetPath(entry.Context), out int id) && id != 0)
                assets.SetSelectionById(id);
            else
                Selection.activeObject = entry.Context;
        }
    }
}
