using UnityEditor;
using UnityEngine.UIElements;

namespace Ripple
{
    [CustomPropertyDrawer(typeof(VariableReferenceBase), true)]
    public class VariableReferenceDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property) => new ReferenceField(property);
    }
}
