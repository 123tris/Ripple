using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Ripple
{
    internal class RippleStyle : ScriptableObject
    {
        public const string Hidden = "ripple-hidden";

        private static StyleSheet loaded;

        [SerializeField] private StyleSheet sheet;

        public static void Apply(VisualElement element)
        {
            if (!loaded)
            {
                RippleStyle holder = CreateInstance<RippleStyle>();
                loaded = holder.sheet;
                DestroyImmediate(holder);
            }

            element.styleSheets.Add(loaded);
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
