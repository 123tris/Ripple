using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Ripple.Tests
{
    public class InspectorTests
    {
        private static IEnumerable<Type> AssetTypes() =>
            TypeCache.GetTypesDerivedFrom<RippleStackTraceSO>().Where(type => !type.IsAbstract && !type.IsGenericType);

        private static IEnumerable<string> Documents() =>
            AssetDatabase.FindAssets("t:VisualTreeAsset", new[] { "Packages/com.metz.ripple/Editor" }).Select(AssetDatabase.GUIDToAssetPath);

        [Test]
        public void EveryAssetType_HasAnInspectorBuiltFromItsLayout([ValueSource(nameof(AssetTypes))] Type type)
        {
            ScriptableObject asset = ScriptableObject.CreateInstance(type);
            Editor editor = Editor.CreateEditor(asset);

            VisualElement inspector = editor.CreateInspectorGUI();

            Assert.IsNotNull(inspector.Q<Foldout>("debug"));
            Assert.IsNull(inspector.Q("stand-in"));
            Object.DestroyImmediate(editor);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void EveryDocument_ShowsItsFieldsInTheBuilder([ValueSource(nameof(Documents))] string path)
        {
            VisualElement document = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path).CloneTree();

            Assert.IsEmpty(document.Query<PropertyField>().ToList(), "UI Builder cannot draw a PropertyField. Use a field with binding-path, or a Ripple control.");
        }

        [Test]
        public void EveryTable_HasItsTemplates([ValueSource(nameof(Documents))] string path)
        {
            VisualElement document = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path).CloneTree();

            IEnumerable<string> columns = document.Query<MultiColumnListView>().ToList()
                .SelectMany(table => table.columns.Where(column => !column.cellTemplate).Select(column => $"{table.name}/{column.name}"));
            IEnumerable<string> lists = document.Query<ListView>("matches").ToList().Where(list => !list.itemTemplate).Select(list => list.name);
            IEnumerable<string> trees = document.Query<TreeView>("assets").ToList().Where(tree => !tree.itemTemplate).Select(tree => tree.name);

            Assert.IsEmpty(columns.Concat(lists).Concat(trees).ToList(), "Declare each cell or row document as a ui:Template, so it imports before this one.");
        }
    }
}
