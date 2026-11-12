using System;
using Unity.Properties;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Ripple
{
    internal static class ValueField
    {
        public static VisualElement Create(Type type, string label)
        {
            VisualElement field =
                type == typeof(float) ? new FloatField(label) :
                type == typeof(int) ? new IntegerField(label) :
                type == typeof(bool) ? new Toggle(label) :
                type == typeof(string) ? new TextField(label) :
                type == typeof(Color) ? new ColorField(label) :
                type == typeof(Vector2) ? new Vector2Field(label) :
                type == typeof(Vector3) ? new Vector3Field(label) :
                type.IsEnum ? new EnumField(label, (Enum)Enum.ToObject(type, 0)) :
                typeof(Object).IsAssignableFrom(type) ? new ObjectField(label) { objectType = type } :
                null;
            field?.AddToClassList(BaseField<int>.alignedFieldUssClassName);
            return field;
        }

        public static VisualElement Slider(Type type, string label)
        {
            VisualElement slider = type == typeof(int)
                ? new SliderInt(label) { showInputField = true }
                : new Slider(label) { showInputField = true };
            slider.AddToClassList(BaseField<int>.alignedFieldUssClassName);
            return slider;
        }

        public static void SetRange(VisualElement slider, float low, float high)
        {
            if (slider is SliderInt whole)
            {
                whole.lowValue = (int)low;
                whole.highValue = (int)high;
            }
            else if (slider is Slider real)
            {
                real.lowValue = low;
                real.highValue = high;
            }
        }

        public static void Bind(VisualElement field, object source, PropertyPath path) =>
            field.SetBinding("value", new DataBinding { dataSource = source, dataSourcePath = path, bindingMode = BindingMode.TwoWay });

        public static object Read(VisualElement field) => field.GetType().GetProperty("value").GetValue(field);
    }
}
