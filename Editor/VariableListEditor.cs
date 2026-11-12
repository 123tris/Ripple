using System;
using System.Collections;
using Unity.Properties;
using UnityEditor;
using UnityEngine.UIElements;

namespace Ripple
{
    [CustomEditor(typeof(VariableListSO), true), CanEditMultipleObjects]
    public class VariableListEditor : RippleEditor
    {
        private VisualElement initial;
        private VisualElement live;
        private ListView values;
        private IList items;
        private int shown;

        protected override void Build(VisualElement root)
        {
            Type type = target.GetType().ArgumentOf(typeof(VariableListSO<>));
            items = (IList)((BaseVariable)target).Value;
            initial = root.Q("initial");
            live = root.Q("live");

            values = root.Q<ListView>("values");
            values.itemsSource = items;
            values.makeItem = () => ValueField.Create(type, null) ?? new Label();
            values.bindItem = (element, index) =>
            {
                if (element is Label label)
                    label.text = items[index]?.ToString();
                else
                    ValueField.Bind(element, items, PropertyPath.FromIndex(index));
            };

            root.Q<Button>("reset").clicked += () => ((BaseVariable)target).ResetValue();
        }

        protected override void Show(bool playing)
        {
            initial.EnableInClassList(RippleStyle.Hidden, playing);
            live.EnableInClassList(RippleStyle.Hidden, !playing);
        }

        protected override void Tick()
        {
            if (shown == items.Count) return;
            shown = items.Count;
            values.RefreshItems();
        }
    }
}
