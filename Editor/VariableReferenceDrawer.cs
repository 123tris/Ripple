using System;
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
        private readonly string[] popupOptions = { "Use Constant", "Use Variable" };

        /// <summary> Cached style to use to draw the popup button. </summary>
        private GUIStyle popupStyle;

        /// <summary> Hint used to allocate a stable control id for the label drag-scrub interaction. </summary>
        private static readonly int DragHash = "RippleVariableReferenceDrag".GetHashCode();

        // Drag state for the label scrub, mirroring EditorGUI.DragNumberValue. Only one drag is
        // ever active at a time, so tracking it in statics (as Unity itself does) is fine.
        private static double dragStartValue;
        private static double dragAccumulated;
        private static double dragSensitivity;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (popupStyle == null)
            {
                popupStyle = new GUIStyle(GUI.skin.GetStyle("PaneOptions"));
                popupStyle.imagePosition = ImagePosition.ImageOnly;
            }

            label = EditorGUI.BeginProperty(position, label, property);

            // Remember the label's rect before PrefixLabel consumes it, so a numeric constant can be
            // scrubbed by dragging over the label just like a native float/int field.
            Rect fullPosition = position;
            position = EditorGUI.PrefixLabel(position, label);
            Rect labelRect = new(fullPosition.x, fullPosition.y, position.x - fullPosition.x, fullPosition.height);

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

            // Let the label scrub the constant when it's a number, matching native inspector behaviour.
            if (useConstant.boolValue)
                DragNumberValue(labelRect, constantValue);

            if (EditorGUI.EndChangeCheck())
                property.serializedObject.ApplyModifiedProperties();

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }

        /// <summary>
        /// Turns <paramref name="dragHotZone"/> into a horizontal number scrubber for the given
        /// property, mirroring the built-in <c>EditorGUI.DragNumberValue</c> used by float/int fields.
        /// Does nothing for non-numeric properties.
        /// </summary>
        private static void DragNumberValue(Rect dragHotZone, SerializedProperty value)
        {
            bool isFloat = value.propertyType == SerializedPropertyType.Float;   // float and double
            bool isInt = value.propertyType == SerializedPropertyType.Integer;   // int and long
            if (!isFloat && !isInt)
                return;

            Event evt = Event.current;
            int id = GUIUtility.GetControlID(DragHash, FocusType.Passive);

            // Show the horizontal-resize cursor over the label so it reads as scrubbable.
            EditorGUIUtility.AddCursorRect(dragHotZone, MouseCursor.SlideArrow);

            switch (evt.GetTypeForControl(id))
            {
                case EventType.MouseDown:
                    if (evt.button == 0 && dragHotZone.Contains(evt.mousePosition))
                    {
                        GUIUtility.hotControl = id;
                        GUIUtility.keyboardControl = 0;
                        dragStartValue = isFloat ? value.doubleValue : value.longValue;
                        dragAccumulated = 0.0;
                        dragSensitivity = CalcDragSensitivity(dragStartValue);
                        evt.Use();
                        EditorGUIUtility.SetWantsMouseJumping(1);
                    }
                    break;

                case EventType.MouseUp:
                    if (GUIUtility.hotControl == id)
                    {
                        GUIUtility.hotControl = 0;
                        evt.Use();
                        EditorGUIUtility.SetWantsMouseJumping(0);
                    }
                    break;

                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == id)
                    {
                        dragAccumulated += HandleUtility.niceMouseDelta * dragSensitivity;
                        double newValue = dragStartValue + dragAccumulated;
                        if (isFloat)
                            value.doubleValue = newValue;
                        else
                            value.longValue = (long)Math.Round(newValue);
                        GUI.changed = true;
                        evt.Use();
                    }
                    break;
            }
        }

        /// <summary>
        /// Drag speed scaled by the value's magnitude, matching Unity's internal
        /// <c>EditorGUI.CalcDragSensitivity</c> so large numbers scrub proportionally faster.
        /// </summary>
        private static double CalcDragSensitivity(double value)
        {
            if (double.IsInfinity(value) || double.IsNaN(value))
                return 0.0;
            return Math.Max(1.0, Math.Pow(Math.Abs(value), 0.5)) * 0.03;
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