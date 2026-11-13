using System;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Ripple
{
    [UxmlElement]
    public partial class SignalField : BindableElement
    {
        private const string Mismatch = "ripple-signal--mismatch";

        private readonly VisualElement standIn;
        private readonly Label header;
        private readonly HelpBox warning;

        public SignalField()
        {
            AddToClassList("ripple-editor");
            AddToClassList("ripple-signal");
            RippleAssets.SignalField.CloneTree(this);
            standIn = this.Q("stand-in");
            header = this.Q<Label>("header");
            warning = this.Q<HelpBox>("warning");
        }

        public SignalField(SerializedProperty property, Type valueType) : this()
        {
            label = property.displayName;
            Configure(property, valueType);
        }

        [UxmlAttribute]
        public string label
        {
            get => header.text;
            set => header.text = value;
        }

        public void Configure(SerializedProperty property, Type valueType)
        {
            SerializedProperty raise = property.FindPropertyRelative("raise");
            standIn.RemoveFromHierarchy();
            Insert(0, new PropertyField(raise, "Also Raise"));
            Insert(0, new PropertyField(property.FindPropertyRelative("response"), label));
            if (valueType == null) return;

            Check(raise);
            this.TrackPropertyValue(raise, Check);

            void Check(SerializedProperty list)
            {
                string[] wrong = Enumerable.Range(0, list.arraySize)
                    .Select(index => list.GetArrayElementAtIndex(index).objectReferenceValue)
                    .Where(gameEvent => gameEvent && gameEvent is not VoidEventSO && gameEvent.GetType().ArgumentOf(typeof(GameEvent<>)) != valueType)
                    .Select(gameEvent => gameEvent.name)
                    .ToArray();
                warning.text = $"{string.Join(", ", wrong)} cannot be raised with a {valueType.Name}. Use an event that takes a {valueType.Name}, or a void event.";
                EnableInClassList(Mismatch, wrong.Length > 0);
            }
        }
    }
}
