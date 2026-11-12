using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Ripple.Tests
{
    public class InspectorTests
    {
        private static IEnumerable<Type> AssetTypes() =>
            TypeCache.GetTypesWithAttribute<RippleData>().Where(type => !type.IsAbstract && !type.IsGenericType);

        [Test]
        public void EveryAssetType_HasAnInspectorBuiltFromItsLayout([ValueSource(nameof(AssetTypes))] Type type)
        {
            ScriptableObject asset = ScriptableObject.CreateInstance(type);
            Editor editor = Editor.CreateEditor(asset);

            VisualElement inspector = editor.CreateInspectorGUI();

            Assert.IsNotNull(inspector.Q<Foldout>("debug"));
            Object.DestroyImmediate(editor);
            Object.DestroyImmediate(asset);
        }
    }
}
