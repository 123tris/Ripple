using System.IO;
using UnityEditor;
using UnityEngine;

namespace Ripple.Editor
{
    [InitializeOnLoad]
    internal static class RippleCreateNewContextMenu
    {
        static RippleCreateNewContextMenu()
        {
            EditorApplication.contextualPropertyMenu += OnPropertyContext;
        }

        private static void OnPropertyContext(GenericMenu menu, SerializedProperty property)
        {
            if (property.propertyType != SerializedPropertyType.ObjectReference) return;
            if (property.objectReferenceValue != null) return;

            var fieldType = ResolveFieldType(property);
            if (fieldType == null || fieldType.IsAbstract) return;
            if (!typeof(RippleStackTraceSO).IsAssignableFrom(fieldType)) return;

            var prop = property.Copy();
            menu.AddItem(new GUIContent($"Create new {fieldType.Name}..."), false, () => CreateAndAssign(prop, fieldType));
        }

        private static System.Type ResolveFieldType(SerializedProperty property)
        {
            var owner = property.serializedObject.targetObject;
            if (owner == null) return null;
            var ownerType = owner.GetType();
            var path = property.propertyPath.Replace(".Array.data[", "[");
            foreach (var token in path.Split('.'))
            {
                var name = token.Contains('[') ? token.Substring(0, token.IndexOf('[')) : token;
                var field = ownerType.GetField(name,
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                if (field == null) return null;
                ownerType = field.FieldType;
                if (ownerType.IsArray) ownerType = ownerType.GetElementType();
            }
            return ownerType;
        }

        private static void CreateAndAssign(SerializedProperty property, System.Type type)
        {
            var so = ScriptableObject.CreateInstance(type);
            so.name = type.Name;
            const string dir = "Assets/RippleAssets";
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            var path = AssetDatabase.GenerateUniqueAssetPath($"{dir}/{type.Name}.asset");
            AssetDatabase.CreateAsset(so, path);
            AssetDatabase.SaveAssets();
            property.objectReferenceValue = so;
            property.serializedObject.ApplyModifiedProperties();
            EditorGUIUtility.PingObject(so);
        }
    }
}
