using System;
using System.Reflection;
using UnityEditor;
using UnityEngine.UIElements;

namespace Ripple
{
    [CustomEditor(typeof(GameEvent), true), CanEditMultipleObjects]
    public class EventEditor : RippleEditor
    {
        private Type type;
        private VisualElement row;
        private VisualElement argument;

        protected override void Build(VisualElement root)
        {
            type = target.GetType().ArgumentOf(typeof(GameEvent<>));
            row = root.Q("raiseRow");
            root.Q<Button>("raise").clicked += Raise;

            VisualElement slot = root.Q("argument");
            slot.Clear();
            if (type != null)
                argument = ValueField.Create(type, null);
            if (argument == null) return;

            argument.AddToClassList("ripple-grow");
            slot.Add(argument);
        }

        protected override void Show(bool playing) => row.SetEnabled(playing);

        private void Raise()
        {
            MethodInfo raise = target.GetType().GetMethod("Raise", type == null ? Type.EmptyTypes : new[] { type });
            raise.Invoke(target, type == null ? null : new[] { Argument() });
        }

        private object Argument()
        {
            if (argument != null) return ValueField.Read(argument);
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }
    }
}
