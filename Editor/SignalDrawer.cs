using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Ripple
{
    [CustomPropertyDrawer(typeof(Signal)), CustomPropertyDrawer(typeof(Signal<>))]
    public class SignalDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement();
            root.Add(new PropertyField(property.FindPropertyRelative("response"), property.displayName));
            root.Add(new PropertyField(property.FindPropertyRelative("raise"), "Also Raise"));
            return root;
        }
    }
}
