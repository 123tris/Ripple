#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace Ripple.EditorTools
{
    public class RippleTypeGenerator : OdinEditorWindow
    {
        [MenuItem("Tools/Ripple/Type Generator")]
        private static void Open()
        {
            GetWindow<RippleTypeGenerator>().Show();
        }

        [BoxGroup("Type Selection")]
        [LabelText("Variable Type")]
        [ShowInInspector]
        public Type SelectedType;

        [BoxGroup("Settings")]
        public string TargetNamespace = "Ripple";

        [BoxGroup("Settings")]
        [FolderPath(ParentFolder = "Assets")]
        public string OutputFolder = "Assets/Scripts/Generated";

        [BoxGroup("Generate")]
        public bool GenerateVariable = true;

        [BoxGroup("Generate")]
        public bool GenerateEvent = true;

        [BoxGroup("Generate")]
        public bool GenerateEventListener = true;

        [BoxGroup("Type Selection")]
        [ShowInInspector, DisplayAsString, ShowIf(nameof(SelectedType))]
        string TypeName => SelectedType?.Name;

        [BoxGroup("Type Selection")]
        [ShowInInspector, DisplayAsString, ShowIf(nameof(SelectedType))]
        string safeTypeName => GetSafeTypeName(SelectedType);

        [BoxGroup("Type Selection")]
        [ShowInInspector, DisplayAsString, ShowIf(nameof(SelectedType))]
        string genericType => SelectedType?.FullName.Replace('+', '.');

        [Button(ButtonSizes.Large)]
        private void Generate()
        {
            if (SelectedType == null)
            {
                Debug.LogError("No type selected.");
                return;
            }

            if (!Directory.Exists(OutputFolder))
                Directory.CreateDirectory(OutputFolder);

            string typeName = SelectedType.Name;
            string safeTypeName = GetSafeTypeName(SelectedType);
            string genericType = SelectedType?.FullName.Replace('+', '.'); //A nested enum will show as "NestedType+EnumType"

            if (GenerateVariable)
                CreateFile(typeName, $"{safeTypeName}VariableSO",
                    GenerateVariableClass(safeTypeName, typeName, genericType));

            if (GenerateEvent)
                CreateFile(typeName, $"{safeTypeName}Event",
                    GenerateEventClass(safeTypeName, typeName, genericType));

            if (GenerateEventListener)
                CreateFile(typeName, $"EventListener{safeTypeName}",
                    GenerateEventListenerClass(safeTypeName, typeName, genericType));

            AssetDatabase.Refresh();
            Debug.Log("Generation complete.");
        }

        // -----------------------------
        // Class Templates
        // -----------------------------

        private string GenerateVariableClass(string safeName, string menuName, string genericType)
        {
            return $@"using UnityEngine;

namespace {TargetNamespace}
{{
    [RippleData]
    [CreateAssetMenu(menuName = Config.VariableMenu + ""{menuName}"")]
    public class {safeName}VariableSO : VariableSO<{genericType}> {{ }}
}}";
        }

        private string GenerateEventClass(string safeName, string menuName, string genericType)
        {
            return $@"using UnityEngine;

namespace {TargetNamespace}
{{
    [CreateAssetMenu(menuName = Config.EventMenu + ""{menuName}"")]
    public class {safeName}Event : GameEvent<{genericType}> {{ }}
}}";
        }

        private string GenerateEventListenerClass(string safeName, string menuName, string genericType)
        {
            return $@"using UnityEngine;

namespace {TargetNamespace}
{{
    [AddComponentMenu(Config.EventListenerMenu + ""Event Listener {menuName}"")]
    public class EventListener{safeName} : EventListener<{genericType}> {{ }}
}}";
        }

        // -----------------------------
        // Helpers
        // -----------------------------

        private void CreateFile(string folderName, string className, string contents)
        {
            string path = Path.Combine(OutputFolder + $"/{folderName}", className + ".cs");

            if (File.Exists(path))
            {
                Debug.LogWarning($"Skipped existing file: {className}");
                return;
            }

            File.WriteAllText(path, contents);
        }

        private static string GetSafeTypeName(Type type)
        {
            if (type == null)
                return "";
            if (type == typeof(int)) return "Int";
            if (type == typeof(float)) return "Float";
            if (type == typeof(bool)) return "Bool";
            if (type == typeof(string)) return "String";

            return type.Name.Replace("`1", "");
        }

        private static List<Type> filteredTypeList;

        //private static IEnumerable<Type> GetFilteredTypes()
        //{
        //    if (filteredTypeList == null)
        //    {
        //        filteredTypeList = AppDomain.CurrentDomain
        //            .GetAssemblies()
        //            .SelectMany(a => a.GetTypes()).ToList();
        //    }

        //    return filteredTypeList;
        //    //.Where(t =>
        //    //    t.IsPublic &&
        //    //    !t.IsAbstract &&
        //    //    !t.IsGenericType &&
        //    //    !t.IsInterface &&
        //    //    !t.IsNested);
        //}

        private static IEnumerable<ValueDropdownItem<Type>> GetTypes()
        {
            return AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch { return Array.Empty<Type>(); }
                })
                .Where(t =>
                    t.IsPublic &&
                    !t.IsAbstract &&
                    !t.IsGenericType &&
                    !t.IsInterface &&
                    !t.IsNested)
                .OrderBy(t => t.Namespace)
                .ThenBy(t => t.Name)
                .Select(t => new ValueDropdownItem<Type>(
                    $"{t.Namespace}.{t.Name}",
                    t));
        }
    }
}
#endif