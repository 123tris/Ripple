using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

namespace Ripple
{
    public abstract class RippleEditor : Editor
    {
        private static readonly Regex location = new(@"\(at (.+):(\d+)\)$");

        [SerializeField] private VisualTreeAsset layout;

        private List<RippleConnections.Listener> listeners = new();
        private MultiColumnListView history;
        private MultiColumnListView listenerTable;
        private ScrollView trace;
        private bool playing;

        private RippleStackTraceSO Asset => (RippleStackTraceSO)target;

        private bool Live => Application.isPlaying && targets.Length == 1;

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = layout.CloneTree();
            Build(root);
            BuildDebug(root.Q<Foldout>("debug"));

            playing = Live;
            Show(playing);
            FindListeners(false);
            Refresh();
            root.schedule.Execute(Refresh).Every(250);
            return root;
        }

        protected abstract void Build(VisualElement root);

        protected abstract void Show(bool playing);

        protected virtual void Tick() { }

        protected void ShowAgain() => Show(playing);

        private void BuildDebug(Foldout debug)
        {
            debug.EnableInClassList(RippleStyle.Hidden, targets.Length > 1);

            history = debug.Q<MultiColumnListView>("history");
            history.itemsSource = Asset.Invokers;
            Fill(history, "caller", row => Asset.Invokers[row].Caller);
            Fill(history, "count", row => Asset.Invokers[row].Count.ToString());
            Fill(history, "value", row => Asset.Invokers[row].LastValue);
            Fill(history, "time", row => Asset.Invokers[row].LastTime);
            history.selectionChanged += _ => ShowTrace();

            trace = debug.Q<ScrollView>("trace");

            listenerTable = debug.Q<MultiColumnListView>("listeners");
            Fill(listenerTable, "kind", row => listeners[row].Kind);
            Fill(listenerTable, "name", row => listeners[row].Name);
            listenerTable.selectionChanged += _ =>
            {
                if (listenerTable.selectedIndex >= 0)
                    EditorGUIUtility.PingObject(listeners[listenerTable.selectedIndex].Target);
            };

            debug.Q<Button>("findListeners").clicked += () => FindListeners(false);
            debug.Q<Button>("searchPrefabs").clicked += () => FindListeners(true);
        }

        private static void Fill(MultiColumnListView table, string column, Func<int, string> text)
        {
            table.columns[column].makeCell = () =>
            {
                var cell = new Label();
                cell.AddToClassList("ripple-cell");
                return cell;
            };
            table.columns[column].bindCell = (cell, row) => ((Label)cell).text = text(row);
        }

        private void Refresh()
        {
            if (playing != Live)
            {
                playing = Live;
                Show(playing);
                FindListeners(false);
            }

            history.EnableInClassList(RippleStyle.Hidden, Asset.Invokers.Count == 0);
            history.RefreshItems();
            Tick();
        }

        private void FindListeners(bool includePrefabs)
        {
            listeners = RippleConnections.Find(Asset, includePrefabs);
            listenerTable.itemsSource = listeners;
            listenerTable.EnableInClassList(RippleStyle.Hidden, listeners.Count == 0);
            listenerTable.RefreshItems();
        }

        private void ShowTrace()
        {
            trace.Clear();
            if (history.selectedItem is not RippleStackTraceSO.InvokeRecord record) return;

            EditorGUIUtility.PingObject(record.Sender);
            if (record.Trace == null) return;

            foreach (string frame in record.Trace.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                var line = new Label(frame);
                line.AddToClassList("ripple-trace__frame");
                Match match = location.Match(frame);
                if (match.Success)
                {
                    line.AddToClassList("ripple-trace__frame--link");
                    line.RegisterCallback<ClickEvent>(_ => InternalEditorUtility.OpenFileAtLineExternal(
                        Path.GetFullPath(match.Groups[1].Value), int.Parse(match.Groups[2].Value)));
                }

                trace.Add(line);
            }
        }
    }
}
