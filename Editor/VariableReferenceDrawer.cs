using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Ripple
{
    [CustomPropertyDrawer(typeof(VariableReferenceBase))]
    public class VariableReferenceDrawer : PropertyDrawer
    {
        /// <summary>
        /// Options to display in the popup to select constant or variable.
        /// </summary>
        private readonly string[] popupOptions = { "Use Constant", "Use Variable", "Use Random" };

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
            SerializedProperty useRandom = property.FindPropertyRelative("useRandom");
            SerializedProperty constantValue = property.FindPropertyRelative("_constantValue");
            SerializedProperty randomValue = property.FindPropertyRelative("_randomValue");
            SerializedProperty variable = property.FindPropertyRelative("_variable");

            // Calculate rect for configuration button
            Rect buttonRect = new(position);
            buttonRect.yMin += popupStyle.margin.top;
            buttonRect.width = popupStyle.fixedWidth + popupStyle.margin.right;
            position.xMin = buttonRect.xMax;

            // Store old indent level and set it to 0, the PrefixLabel takes care of it
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            int selectedIndex = useRandom.boolValue ? 2 : useConstant.boolValue ? 0 : 1;
            
            int result = EditorGUI.Popup(buttonRect, selectedIndex, popupOptions, popupStyle);

            useConstant.boolValue = result == 0;
            useRandom.boolValue = result == 2;

            SerializedProperty propertyToDraw;
            if (useRandom.boolValue)
                propertyToDraw = randomValue;
            else if (useConstant.boolValue)
                propertyToDraw = constantValue;
            else
                propertyToDraw = variable;
            EditorGUI.PropertyField(position, propertyToDraw, GUIContent.none);

            if (EditorGUI.EndChangeCheck())
                property.serializedObject.ApplyModifiedProperties();

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }
    }

    //public class VariableReferenceDrawerOdin<T> : OdinValueDrawer<T> where T : VariableReferenceBase
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
    //        EditorGUILayout.BeginHorizontal();
    //        rect = EditorGUI.PrefixLabel(rect, label);

    //        // Calculate rect for configuration button
    //        Rect buttonRect = new(rect);
    //        buttonRect.yMin += _popupStyle.margin.top;
    //        buttonRect.width = _popupStyle.fixedWidth + _popupStyle.margin.right;
    //        rect.xMin = buttonRect.xMax;

    //        bool useConstant = (bool) _useConstant.ValueEntry.WeakSmartValue;

    //        int result = EditorGUI.Popup(buttonRect, useConstant ? 0 : 1, _popupOptions, _popupStyle);

    //        _useConstant.ValueEntry.WeakSmartValue = result == 0;
    //        if (useConstant)
    //            _constantValue.Draw(GUIContent.none);
    //        else
    //            _variable.Draw(GUIContent.none);
    //        EditorGUILayout.EndHorizontal();
    //    }
    //}
}