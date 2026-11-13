using System;
using System.Collections.Generic;
using System.Linq;
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
            SerializedProperty raise = property.FindPropertyRelative("raise");
            var root = new VisualElement();
            RippleStyle.Apply(root);
            root.Add(new PropertyField(property.FindPropertyRelative("response"), property.displayName));
            root.Add(new PropertyField(raise, "Also Raise"));

            Type value = SignalType().ArgumentOf(typeof(Signal<>));
            if (value == null) return root;

            var warning = new HelpBox("", HelpBoxMessageType.Warning);
            root.Add(warning);
            Check(raise);
            root.TrackPropertyValue(raise, Check);
            return root;

            void Check(SerializedProperty list)
            {
                string[] wrong = Enumerable.Range(0, list.arraySize)
                    .Select(index => list.GetArrayElementAtIndex(index).objectReferenceValue)
                    .Where(gameEvent => gameEvent && gameEvent is not VoidEventSO && gameEvent.GetType().ArgumentOf(typeof(GameEvent<>)) != value)
                    .Select(gameEvent => gameEvent.name)
                    .ToArray();
                warning.text = $"{string.Join(", ", wrong)} cannot be raised with a {value.Name}. Use an event that takes a {value.Name}, or a void event.";
                warning.EnableInClassList(RippleStyle.Hidden, wrong.Length == 0);
            }
        }

        private Type SignalType()
        {
            Type type = fieldInfo.FieldType;
            if (type.IsArray) return type.GetElementType();
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>) ? type.GetGenericArguments()[0] : type;
        }
    }
}
