using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Ripple.Editor
{
    [CustomEditor(typeof(RippleStackTraceSO), true)]
    public class RippleStackTraceSOEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Ripple Debug", EditorStyles.boldLabel);

            var t = target as RippleStackTraceSO;
            if (t == null) return;

            if (t is GameEvent ge)
            {
                EditorGUILayout.LabelField("Subscribers", ge.SubscriberCount.ToString());
                if (Application.isPlaying)
                {
                    if (GUILayout.Button("Raise (default payload)"))
                    {
                        var invoke = t.GetType().GetMethod("Invoke", new[] { ge.PayloadType, typeof(string), typeof(string), typeof(int) });
                        if (invoke != null)
                        {
                            object payload = ge.PayloadType.IsValueType ? System.Activator.CreateInstance(ge.PayloadType) : null;
                            invoke.Invoke(t, new[] { payload, "RaiseFromInspector", "RippleStackTraceSOEditor", 0 });
                        }
                    }
                }
            }
            else if (t is BaseVariable bv)
            {
                EditorGUILayout.LabelField("Subscribers", bv.SubscriberCount.ToString());
                EditorGUILayout.LabelField("Current Value", bv.BoxedValue?.ToString() ?? "null");
            }

            var historyProp = typeof(RippleStackTraceSO).GetProperty("History",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (historyProp != null)
            {
                var history = historyProp.GetValue(t) as System.Collections.IEnumerable;
                if (history != null)
                {
                    EditorGUILayout.LabelField("Recent Invokes", EditorStyles.boldLabel);
                    int count = 0;
                    foreach (var entry in history)
                    {
                        if (count++ > 32) { EditorGUILayout.LabelField("...older trimmed..."); break; }
                        EditorGUILayout.LabelField(entry.ToString());
                    }
                }
            }
        }
    }
}
