using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Ripple.Editor
{
    public sealed class RippleDebuggerWindow : EditorWindow
    {
        private enum Tab { Inventory, LiveLog, Trace }

        [MenuItem("Tools/Ripple/Debugger")]
        public static void Open()
        {
            GetWindow<RippleDebuggerWindow>("Ripple Debugger").Show();
        }

        private Tab _tab = Tab.Inventory;
        private Vector2 _scroll;
        private string _filter = string.Empty;
        private RippleStackTraceSO _selected;
        private Guid _selectedTrace;

        private void OnEnable()
        {
            RippleLogger.OnEntryAdded += OnEntryAdded;
            TraceStore.OnTraceCommitted += OnTraceCommitted;
        }

        private void OnDisable()
        {
            RippleLogger.OnEntryAdded -= OnEntryAdded;
            TraceStore.OnTraceCommitted -= OnTraceCommitted;
        }

        private void OnEntryAdded(RippleLogger.Entry e) => Repaint();
        private void OnTraceCommitted(Guid id, IReadOnlyList<TraceFrame> frames) => Repaint();

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            foreach (Tab tab in Enum.GetValues(typeof(Tab)))
            {
                if (GUILayout.Toggle(_tab == tab, tab.ToString(), EditorStyles.toolbarButton)) _tab = tab;
            }
            GUILayout.FlexibleSpace();
            _filter = GUILayout.TextField(_filter, EditorStyles.toolbarSearchField, GUILayout.Width(200));
            if (GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                RippleLogger.Clear();
                TraceStore.Clear();
            }
            EditorGUILayout.EndHorizontal();

            switch (_tab)
            {
                case Tab.Inventory: DrawInventory(); break;
                case Tab.LiveLog: DrawLiveLog(); break;
                case Tab.Trace: DrawTrace(); break;
            }
        }

        private void DrawInventory()
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical(GUILayout.Width(position.width * 0.45f));
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            var allChannels = Resources.FindObjectsOfTypeAll<RippleStackTraceSO>()
                .Where(c => string.IsNullOrEmpty(_filter) || c.name.IndexOf(_filter, StringComparison.OrdinalIgnoreCase) >= 0)
                .OrderBy(c => c.GetType().Name).ThenBy(c => c.name);

            string lastGroup = null;
            foreach (var ch in allChannels)
            {
                var group = ch is GameEvent ? "Events" : ch is BaseVariable ? "Variables" : "Other";
                if (group != lastGroup)
                {
                    EditorGUILayout.LabelField(group, EditorStyles.boldLabel);
                    lastGroup = group;
                }
                int subCount = ch is IChannel ich ? ich.SubscriberCount : 0;
                if (GUILayout.Button($"{ch.name}  ({ch.GetType().Name}, {subCount} subs)", EditorStyles.miniButton))
                    _selected = ch;
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            DrawSelectedDetails();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }

        private void DrawSelectedDetails()
        {
            if (_selected == null)
            {
                EditorGUILayout.HelpBox("Select a channel from the left.", MessageType.Info);
                return;
            }
            EditorGUILayout.LabelField(_selected.name, EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Type", _selected.GetType().Name);

            if (_selected is IChannel ich)
                EditorGUILayout.LabelField("Subscribers", ich.SubscriberCount.ToString());

            if (_selected is BaseVariable bv)
                EditorGUILayout.LabelField("Current value", bv.BoxedValue?.ToString() ?? "null");

            if (GUILayout.Button("Ping in Project")) EditorGUIUtility.PingObject(_selected);
            if (GUILayout.Button("Select")) Selection.activeObject = _selected;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Find Usages", EditorStyles.boldLabel);
            if (GUILayout.Button("Find references"))
            {
                var usages = ReferenceUsageIndex.FindUsages(_selected);
                if (usages.Count == 0) Debug.Log("[Ripple] No usages found.", _selected);
                else Debug.Log($"[Ripple] {usages.Count} usage(s):\n  " + string.Join("\n  ", usages), _selected);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Recent invokes", EditorStyles.boldLabel);
            int shown = 0;
            foreach (var entry in _selected.History)
            {
                if (shown++ > 16) { EditorGUILayout.LabelField("...older trimmed..."); break; }
                EditorGUILayout.LabelField($"f{entry.frame} {entry.callSite} = {entry.payloadString}");
            }
        }

        private void DrawLiveLog()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            foreach (var e in RippleLogger.Entries)
            {
                if (!string.IsNullOrEmpty(_filter) && !e.sourceName.Contains(_filter, StringComparison.OrdinalIgnoreCase)) continue;
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(e.sourceName, EditorStyles.miniButtonLeft, GUILayout.Width(180)))
                {
                    if (e.source != null) EditorGUIUtility.PingObject(e.source);
                }
                EditorGUILayout.LabelField($"f{e.frame}  {e.callSite}  ⇒  {e.payloadString}", GUILayout.MinWidth(200));
                if (e.traceId != Guid.Empty)
                {
                    if (GUILayout.Button($"trace#{e.hopIndex}", EditorStyles.miniButtonRight, GUILayout.Width(80)))
                    {
                        _tab = Tab.Trace;
                        _selectedTrace = e.traceId;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawTrace()
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical(GUILayout.Width(position.width * 0.4f));
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            EditorGUILayout.LabelField("Recent traces", EditorStyles.boldLabel);
            foreach (var kv in TraceStore.CommittedTraces)
            {
                var first = kv.Value.Count > 0 ? kv.Value[0] : default;
                var label = $"{first.SourceName} @f{first.FrameNumber} ({kv.Value.Count} hops)";
                if (GUILayout.Button(label, _selectedTrace == kv.Key ? EditorStyles.helpBox : EditorStyles.miniButton))
                    _selectedTrace = kv.Key;
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            var frames = TraceStore.GetTrace(_selectedTrace);
            if (frames == null)
            {
                EditorGUILayout.HelpBox("Select a trace from the left.", MessageType.Info);
            }
            else
            {
                EditorGUILayout.LabelField("Trace", _selectedTrace.ToString(), EditorStyles.boldLabel);
                foreach (var f in frames)
                {
                    EditorGUI.indentLevel = f.HopIndex;
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button(f.SourceName, EditorStyles.miniButton, GUILayout.Width(180)))
                    {
                        if (f.Source != null) EditorGUIUtility.PingObject(f.Source);
                    }
                    EditorGUILayout.LabelField($"⇒ {f.PayloadString}  ({f.CallerMember}:{f.CallerLine})");
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUI.indentLevel = 0;
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }
    }
}
