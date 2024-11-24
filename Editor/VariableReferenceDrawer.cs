using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Ripple
{
    [CustomPropertyDrawer(typeof(VariableReferenceBase))]
    public class VariableReferenceDrawer : PropertyDrawer
    {
        /// <summary>
        /// Options to display in the popup to select constant or variable.
        /// </summary>
        private readonly string[] popupOptions =
            { "Use Constant", "Use Variable" };

        /// <summary> Cached style to use to draw the popup button. </summary>
        private GUIStyle popupStyle;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (popupStyle == null)
            {
                popupStyle = new GUIStyle(GUI.skin.GetStyle("PaneOptions"));
                popupStyle.imagePosition = ImagePosition.ImageOnly;
            }

            label = EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, label);

            EditorGUI.BeginChangeCheck();

            // Get properties
            SerializedProperty useConstant = property.FindPropertyRelative("useConstant");
            SerializedProperty constantValue = property.FindPropertyRelative("_constantValue");
            SerializedProperty variable = property.FindPropertyRelative("_variable");

            // Calculate rect for configuration button
            Rect buttonRect = new(position);
            buttonRect.yMin += popupStyle.margin.top;
            buttonRect.width = popupStyle.fixedWidth + popupStyle.margin.right;
            position.xMin = buttonRect.xMax;

            // Store old indent level and set it to 0, the PrefixLabel takes care of it
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            int result = EditorGUI.Popup(buttonRect, useConstant.boolValue ? 0 : 1, popupOptions, popupStyle);

            useConstant.boolValue = result == 0;

            EditorGUI.PropertyField(position, useConstant.boolValue ? constantValue : variable, GUIContent.none);

            if (EditorGUI.EndChangeCheck())
                property.serializedObject.ApplyModifiedProperties();

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }
    }

    //public class VariableReferenceDrawerOdin<T> : OdinValueDrawer<T> where T : VariableReference<T>
    //{
    //    private InspectorProperty _useConstant;
    //    private InspectorProperty _constantValue;
    //    private InspectorProperty _variable;

    //    /// <summary> Cached style to use to draw the popup button. </summary>
    //    private GUIStyle _popupStyle;

    //    /// <summary>
    //    /// Options to display in the popup to select constant or variable.
    //    /// </summary>
    //    private readonly string[] _popupOptions =
    //        { "Use Constant", "Use Variable" };
    //    protected override void DrawPropertyLayout(GUIContent label)
    //    {
    //        _useConstant = Property.Children["useConstant"];
    //        _constantValue = Property.Children["_constantValue"];
    //        _variable = Property.Children["_variable"];

    //        if (_popupStyle == null)
    //        {
    //            _popupStyle = new GUIStyle(GUI.skin.GetStyle("PaneOptions"));
    //            _popupStyle.imagePosition = ImagePosition.ImageOnly;
    //        }

    //        Rect rect = EditorGUILayout.GetControlRect();
    //        //label = EditorGUI.BeginProperty(rect, label, ValueEntry);
    //        rect = EditorGUI.PrefixLabel(rect, label);

    //        EditorGUI.BeginChangeCheck();

    //        // Calculate rect for configuration button
    //        Rect buttonRect = new(rect);
    //        buttonRect.yMin += _popupStyle.margin.top;
    //        buttonRect.width = _popupStyle.fixedWidth + _popupStyle.margin.right;
    //        rect.xMin = buttonRect.xMax;

    //        // Store old indent level and set it to 0, the PrefixLabel takes care of it
    //        //int indent = EditorGUI.indentLevel;
    //        //EditorGUI.indentLevel = 0;

    //        int result = EditorGUI.Popup(buttonRect, (bool)_useConstant.ValueEntry.WeakSmartValue ? 0 : 1, _popupOptions, _popupStyle);
    //        _useConstant.ValueEntry.WeakSmartValue = result == 0;
    //        //EditorGUI.indentLevel = indent;
    //    }
    //}
}