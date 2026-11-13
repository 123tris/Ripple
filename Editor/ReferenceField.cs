using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Ripple
{
    [UxmlElement]
    public partial class ReferenceField : BindableElement
    {
        private const string VariableMode = "ripple-reference--variable";

        private readonly FloatField standIn;
        private readonly Image menu;

        public ReferenceField()
        {
            AddToClassList("ripple-editor");
            AddToClassList("ripple-reference");
            RippleAssets.ReferenceField.CloneTree(this);
            standIn = this.Q<FloatField>("stand-in");
            menu = this.Q<Image>("menu");
            menu.image = EditorGUIUtility.IconContent("_Menu").image;
        }

        public ReferenceField(SerializedProperty property) : this()
        {
            label = property.displayName;
            Configure(property);
        }

        [UxmlAttribute]
        public string label
        {
            get => standIn.label;
            set => standIn.label = value;
        }

        public void Configure(SerializedProperty property)
        {
            SerializedProperty useConstant = property.FindPropertyRelative("useConstant");
            VisualElement row = standIn.parent;
            row.Insert(0, Field(property.FindPropertyRelative("_variable"), "ripple-reference__variable"));
            row.Insert(0, Field(property.FindPropertyRelative("_constantValue"), "ripple-reference__constant"));
            standIn.RemoveFromHierarchy();

            menu.AddManipulator(new Clickable(() =>
            {
                var choices = new GenericMenu();
                choices.AddItem(new GUIContent("Constant"), useConstant.boolValue, () => Choose(true));
                choices.AddItem(new GUIContent("Variable"), !useConstant.boolValue, () => Choose(false));
                choices.DropDown(menu.worldBound);
            }));

            EnableInClassList(VariableMode, !useConstant.boolValue);
            this.TrackPropertyValue(useConstant, changed => EnableInClassList(VariableMode, !changed.boolValue));

            void Choose(bool isConstant)
            {
                useConstant.boolValue = isConstant;
                useConstant.serializedObject.ApplyModifiedProperties();
            }
        }

        private PropertyField Field(SerializedProperty property, string className)
        {
            var field = new PropertyField(property, label);
            field.AddToClassList("ripple-grow");
            field.AddToClassList(className);
            return field;
        }
    }
}
