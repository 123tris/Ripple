using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Ripple.Editor
{
    [CustomPropertyDrawer(typeof(NumericVariableReference))]
    public sealed class NumericVariableReferenceDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var useConstant = property.FindPropertyRelative("_useConstant");
            var constant = property.FindPropertyRelative("_constantValue");
            var variable = property.FindPropertyRelative("_variable");

            EditorGUI.BeginProperty(position, label, property);

            var labelRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, position.height);
            EditorGUI.LabelField(labelRect, label);

            var modeRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, 22f, position.height);
            int currentIndex = useConstant.boolValue ? 0 : 1;
            int newIndex = EditorGUI.Popup(modeRect, currentIndex, new[] { "C", "V" });
            if (newIndex != currentIndex) useConstant.boolValue = newIndex == 0;

            var valueRect = new Rect(modeRect.xMax + 4f, position.y, position.width - EditorGUIUtility.labelWidth - modeRect.width - 4f, position.height);

            if (useConstant.boolValue)
            {
                constant.doubleValue = EditorGUI.DoubleField(valueRect, constant.doubleValue);
            }
            else
            {
                var current = variable.objectReferenceValue;
                var so = (ScriptableObject)EditorGUI.ObjectField(valueRect, current, typeof(ScriptableObject), false);
                if (so != current)
                {
                    if (so == null || so is INumericVariable)
                        variable.objectReferenceValue = so;
                    else
                        Debug.LogWarning($"[Ripple] {so.name} ({so.GetType().Name}) is not an INumericVariable; rejected.", so);
                }
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            => EditorGUIUtility.singleLineHeight;
    }
}
