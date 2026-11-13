using System;
using Unity.Properties;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Ripple
{
    [CustomEditor(typeof(BaseVariable), true), CanEditMultipleObjects]
    public class VariableEditor : RippleEditor
    {
        private const string Clamped = "ripple-variable--clamped";
        private const string Persisted = "ripple-variable--persisted";

        private Type type;
        private VisualElement slot;
        private VisualElement field;
        private SerializedProperty clamped;
        private SerializedProperty min;
        private SerializedProperty max;

        private bool UsesSlider => clamped is { boolValue: true } && (type == typeof(float) || type == typeof(int));

        protected override void Build(VisualElement root)
        {
            type = target.GetType().ArgumentOf(typeof(VariableSO<>));
            slot = root.Q("value");
            root.Q<Button>("reset").clicked += () => ((BaseVariable)target).ResetValue();

            clamped = serializedObject.FindProperty("_isClamped");
            if (clamped == null)
            {
                root.Q(className: "ripple-variable__clamp").RemoveFromHierarchy();
            }
            else
            {
                min = serializedObject.FindProperty("min");
                max = serializedObject.FindProperty("max");
                SetState(Clamped, clamped.boolValue);
                root.TrackPropertyValue(clamped, changed =>
                {
                    SetState(Clamped, changed.boolValue);
                    ShowAgain();
                });
            }

            SerializedProperty persist = serializedObject.FindProperty("_persist");
            SetState(Persisted, persist.boolValue);
            root.TrackPropertyValue(persist, changed => SetState(Persisted, changed.boolValue));
            root.Q<Button>("clearSaved").clicked += () =>
            {
                foreach (BaseVariable variable in targets)
                    variable.ClearSavedValue();
            };
        }

        protected override void Show(bool playing)
        {
            slot.Clear();
            field = playing ? Current() : Initial();
            slot.Add(field);
            Tick();
        }

        protected override void Tick()
        {
            if (field is TextField { isReadOnly: true } text)
                text.value = ((BaseVariable)target).Value?.ToString();
            else if (UsesSlider)
                ValueField.SetRange(field, Bound(min), Bound(max));
        }

        private VisualElement Initial()
        {
            VisualElement initial = UsesSlider ? ValueField.Slider(type, "Initial Value") : new PropertyField { label = "Initial Value" };
            if (UsesSlider)
                ValueField.SetRange(initial, Bound(min), Bound(max));
            ((IBindable)initial).bindingPath = "_initialValue";
            initial.Bind(serializedObject);
            return initial;
        }

        private VisualElement Current()
        {
            VisualElement current = UsesSlider ? ValueField.Slider(type, "Value") : ValueField.Create(type, "Value");
            if (current == null)
                return new TextField("Value") { isReadOnly = true };

            if (UsesSlider)
                ValueField.SetRange(current, Bound(min), Bound(max));
            ValueField.Bind(current, target, new PropertyPath("CurrentValue"));
            return current;
        }

        private static float Bound(SerializedProperty reference)
        {
            if (reference.FindPropertyRelative("useConstant").boolValue)
            {
                SerializedProperty constant = reference.FindPropertyRelative("_constantValue");
                return constant.propertyType == SerializedPropertyType.Float ? constant.floatValue : constant.intValue;
            }

            var variable = reference.FindPropertyRelative("_variable").objectReferenceValue as BaseVariable;
            return variable ? Convert.ToSingle(variable.Value) : 0f;
        }
    }
}
