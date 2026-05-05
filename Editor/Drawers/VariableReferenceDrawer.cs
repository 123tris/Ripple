using UnityEditor;
using UnityEngine;

namespace Ripple.Editor
{
    [CustomPropertyDrawer(typeof(VariableReferenceBase), true)]
    public sealed class VariableReferenceDrawer : PropertyDrawer
    {
        private static readonly string[] Modes = { "Constant", "Variable" };

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var useConstant = property.FindPropertyRelative("_useConstant");
            var constant = property.FindPropertyRelative("_constantValue");
            var variable = property.FindPropertyRelative("_variable");
            if (useConstant == null || constant == null || variable == null)
            {
                EditorGUI.LabelField(position, label.text, "VariableReference fields not found");
                return;
            }

            EditorGUI.BeginProperty(position, label, property);

            var labelRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, position.height);
            EditorGUI.LabelField(labelRect, label);

            var modeRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, 22f, position.height);
            int currentIndex = useConstant.boolValue ? 0 : 1;
            int newIndex = EditorGUI.Popup(modeRect, currentIndex, new[] { "C", "V" });
            if (newIndex != currentIndex) useConstant.boolValue = newIndex == 0;

            var valueRect = new Rect(modeRect.xMax + 4f, position.y, position.width - EditorGUIUtility.labelWidth - modeRect.width - 4f, position.height);
            if (useConstant.boolValue)
                EditorGUI.PropertyField(valueRect, constant, GUIContent.none);
            else
                EditorGUI.PropertyField(valueRect, variable, GUIContent.none);

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            => EditorGUIUtility.singleLineHeight;
    }
}
