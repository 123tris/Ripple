using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Unity.Properties;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

namespace Ripple
{
    public abstract class RippleEditor : Editor
    {
        private const string Playing = "ripple-editor--playing";

        private static readonly Regex location = new(@"\(at (.+):(\d+)\)$");

        [SerializeField] private VisualTreeAsset layout;

        private VisualElement root;
        private Foldout debug;
        private MultiColumnListView history;
        private MultiColumnListView listenerTable;
        private ScrollView trace;
        private bool playing;

        [CreateProperty] internal List<RippleStackTraceSO.InvokeRecord> History => Asset.Invokers;

        [CreateProperty] internal List<RippleConnections.Listener> Listeners { get; } = new();

        private RippleStackTraceSO Asset => (RippleStackTraceSO)target;

        private bool Live => Application.isPlaying && targets.Length == 1;

        public override VisualElement CreateInspectorGUI()
        {
            TemplateContainer document = layout.CloneTree();
            document.dataSource = this;
            root = document.Q(className: "ripple-editor");
            root.EnableInClassList("ripple-editor--multi", targets.Length > 1);
            Build(root);
            ConfigureControls();
            BuildDebug();

            playing = Live;
            ShowPlaying();
            FindListeners(false);
            Refresh();
            root.schedule.Execute(Refresh).Every(250);
            return document;
        }

        protected abstract void Build(VisualElement root);

        protected virtual void Show(bool playing) { }

        protected virtual void Tick() { }

        protected void SetState(string modifier, bool on) => root.EnableInClassList(modifier, on);

        protected void ShowAgain() => Show(playing);

        private void ShowPlaying()
        {
            SetState(Playing, playing);
            Show(playing);
        }

        private void ConfigureControls()
        {
            foreach (ReferenceField field in root.Query<ReferenceField>().ToList())
                field.Configure(serializedObject.FindProperty(field.bindingPath));

            foreach (SignalField field in root.Query<SignalField>().ToList())
                field.Configure(serializedObject.FindProperty(field.bindingPath), target.GetType().FieldTypeOf(field.bindingPath).ArgumentOf(typeof(Signal<>)));
        }

        private void BuildDebug()
        {
            debug = root.Q<Foldout>("debug");
            history = debug.Q<MultiColumnListView>("history");
            history.selectionChanged += _ => ShowTrace();
            trace = debug.Q<ScrollView>("trace");
            listenerTable = debug.Q<MultiColumnListView>("listeners");
            listenerTable.selectionChanged += _ => EditorGUIUtility.PingObject((listenerTable.selectedItem as RippleConnections.Listener)?.Target);
            debug.Q<Button>("findListeners").clicked += () => FindListeners(false);
            debug.Q<Button>("searchPrefabs").clicked += () => FindListeners(true);
        }

        private void Refresh()
        {
            if (playing != Live)
            {
                playing = Live;
                ShowPlaying();
                FindListeners(false);
            }

            debug.EnableInClassList("ripple-debug--history", Asset.Invokers.Count > 0);
            history.RefreshItems();
            Tick();
        }

        private void FindListeners(bool includePrefabs)
        {
            RippleConnections.Find(Asset, includePrefabs, Listeners);
            debug.EnableInClassList("ripple-debug--listeners", Listeners.Count > 0);
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
                line.AddToClassList("ripple-debug__frame");
                Match match = location.Match(frame);
                if (match.Success)
                {
                    line.AddToClassList("ripple-debug__frame--link");
                    line.RegisterCallback<ClickEvent>(_ => InternalEditorUtility.OpenFileAtLineExternal(
                        Path.GetFullPath(match.Groups[1].Value), int.Parse(match.Groups[2].Value)));
                }

                trace.Add(line);
            }
        }
    }
}
