using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Ripple
{
    internal class RippleAssets : ScriptableObject
    {
        private static VisualTreeAsset referenceTemplate;
        private static VisualTreeAsset signalTemplate;

        [SerializeField] private VisualTreeAsset referenceField;
        [SerializeField] private VisualTreeAsset signalField;

        public static VisualTreeAsset ReferenceField => Template(ref referenceTemplate, holder => holder.referenceField);

        public static VisualTreeAsset SignalField => Template(ref signalTemplate, holder => holder.signalField);

        private static VisualTreeAsset Template(ref VisualTreeAsset cached, Func<RippleAssets, VisualTreeAsset> pick)
        {
            if (cached) return cached;

            RippleAssets holder = CreateInstance<RippleAssets>();
            cached = pick(holder);
            DestroyImmediate(holder);
            return cached;
        }

        public static void Restore(EditorWindow window, ref VisualTreeAsset layout)
        {
            if (layout) return;

            var created = (EditorWindow)CreateInstance(window.GetType());
            layout = (VisualTreeAsset)new SerializedObject(created).FindProperty("layout").objectReferenceValue;
            DestroyImmediate(created);
        }
    }
}
