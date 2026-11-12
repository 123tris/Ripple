using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Ripple
{
    [CustomPropertyDrawer(typeof(VariableReferenceBase), true)]
    public class VariableReferenceDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            SerializedProperty useConstant = property.FindPropertyRelative("useConstant");
            var constant = new PropertyField(property.FindPropertyRelative("_constantValue"), property.displayName);
            var variable = new PropertyField(property.FindPropertyRelative("_variable"), property.displayName);
            var mode = new Image { image = EditorGUIUtility.IconContent("_Menu").image, tooltip = "Use a constant or a variable asset." };

            var row = new VisualElement();
            RippleStyle.Apply(row);
            row.AddToClassList("ripple-row");
            constant.AddToClassList("ripple-grow");
            variable.AddToClassList("ripple-grow");
            mode.AddToClassList("ripple-menu");
            row.Add(constant);
            row.Add(variable);
            row.Add(mode);

            mode.AddManipulator(new Clickable(() =>
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("Constant"), useConstant.boolValue, () => Choose(true));
                menu.AddItem(new GUIContent("Variable"), !useConstant.boolValue, () => Choose(false));
                menu.DropDown(mode.worldBound);
            }));

            Show(useConstant.boolValue);
            row.TrackPropertyValue(useConstant, changed => Show(changed.boolValue));
            return row;

            void Show(bool isConstant)
            {
                constant.EnableInClassList(RippleStyle.Hidden, !isConstant);
                variable.EnableInClassList(RippleStyle.Hidden, isConstant);
            }

            void Choose(bool isConstant)
            {
                useConstant.boolValue = isConstant;
                useConstant.serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
