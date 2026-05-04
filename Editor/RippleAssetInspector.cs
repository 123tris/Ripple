using System;
using System.Text;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using Ripple.Editor;

namespace Ripple
{
    public abstract class RippleInspectorBase : OdinEditor
    {
        private Vector2 _referenceScroll;
        private ReferenceUsageSummary _cachedSummary;
        private double _summaryBuiltAt = -1;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            DrawReferenceOverview(target);
        }

        protected abstract string GetSummaryTitle();

        protected virtual void DrawExtraSummaryFields(ReferenceUsageSummary summary, UnityEngine.Object targetObject) { }

        private void DrawReferenceOverview(UnityEngine.Object targetObject)
        {
            var summary = ReferenceUsageUtility.BuildSummary(targetObject);
            _cachedSummary = summary;
            _summaryBuiltAt = EditorApplication.timeSinceStartup;
            DrawSummaryBox(summary, GetSummaryTitle(), targetObject);
        }

        private void DrawSummaryBox(ReferenceUsageSummary summary, string title, UnityEngine.Object targetObject)
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            if (_summaryBuiltAt > 0)
            {
                double age = EditorApplication.timeSinceStartup - _summaryBuiltAt;
                string ageText = age < 2 ? "just now" : $"{age:F0}s ago";
                EditorGUILayout.LabelField($"({ageText})", EditorStyles.miniLabel, GUILayout.Width(70));
            }
            if (GUILayout.Button("Refresh", EditorStyles.miniButton, GUILayout.Width(60)))
            {
                ReferenceUsageUtility.InvalidateCache(targetObject);
                Repaint();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("Usage Mode", summary.Mode.ToString());
            EditorGUILayout.LabelField(GetInvokeLabel(), summary.InvokeReferences.ToString());
            EditorGUILayout.LabelField("Listen references", summary.ListenReferences.ToString());

            DrawExtraSummaryFields(summary, targetObject);

            if (summary.ExampleReferences.Count > 0)
            {
                EditorGUILayout.LabelField("Sample References", EditorStyles.miniBoldLabel);
                _referenceScroll = EditorGUILayout.BeginScrollView(_referenceScroll, GUILayout.Height(90f));
                var sb = new StringBuilder();
                foreach (var reference in summary.ExampleReferences)
                    sb.AppendLine(reference);
                EditorGUILayout.SelectableLabel(sb.ToString(), EditorStyles.textArea, GUILayout.ExpandHeight(true));
                EditorGUILayout.EndScrollView();
            }

            EditorGUILayout.EndVertical();
        }

        protected virtual string GetInvokeLabel() => "Invoke references";
    }

    [CustomEditor(typeof(GameEvent), true)]
    public class GameEventInspector : RippleInspectorBase
    {
        protected override string GetSummaryTitle() => "Event Reference Overview";

        protected override void DrawExtraSummaryFields(ReferenceUsageSummary summary, UnityEngine.Object targetObject)
        {
            var gameEvent = targetObject as GameEvent;
            if (gameEvent != null)
                EditorGUILayout.LabelField("Runtime listeners", gameEvent.RuntimeListenerCount.ToString());
        }
    }

    [CustomEditor(typeof(BaseVariable), true)]
    public class BaseVariableInspector : RippleInspectorBase
    {
        protected override string GetSummaryTitle() => "Variable Reference Overview";

        protected override string GetInvokeLabel() => "Write/Invoke references";
    }
}
