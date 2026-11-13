using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

namespace Ripple
{
    [CustomPropertyDrawer(typeof(Signal)), CustomPropertyDrawer(typeof(Signal<>))]
    public class SignalDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property) =>
            new SignalField(property, SignalType().ArgumentOf(typeof(Signal<>)));

        private Type SignalType()
        {
            Type type = fieldInfo.FieldType;
            if (type.IsArray) return type.GetElementType();
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>) ? type.GetGenericArguments()[0] : type;
        }
    }
}
