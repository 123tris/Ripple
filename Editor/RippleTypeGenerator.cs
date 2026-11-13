using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Properties;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Ripple
{
    public class RippleTypeGenerator : EditorWindow
    {
        [Flags]
        private enum Parts
        {
            Variable = 1,
            VariableListener = 2,
            Event = 4,
            EventListener = 8,
            List = 16,
        }

        private static Type[] all;

        [SerializeField] private VisualTreeAsset layout;
        [SerializeField] private string folder = "Assets/Ripple";
        [SerializeField] private string @namespace;
        [SerializeField] private Parts parts = Parts.Variable | Parts.VariableListener | Parts.Event | Parts.EventListener | Parts.List;

        private Type type;

        [CreateProperty] internal List<TypeEntry> Matches { get; } = new();

        internal sealed class TypeEntry
        {
            public string FullName;
            public Type Type;
        }

        [MenuItem("Tools/Ripple/Type Generator")]
        private static void Open() => GetWindow<RippleTypeGenerator>("Ripple Type Generator");

        private void OnEnable()
        {
            if (string.IsNullOrEmpty(@namespace))
                @namespace = EditorSettings.projectGenerationRootNamespace;
        }

        private void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            RippleAssets.Restore(this, ref layout);
            layout.CloneTree(root);
            root.dataSource = this;
            root.Bind(new SerializedObject(this));

            all ??= TypeCache.GetTypesDerivedFrom<object>()
                .Concat(typeof(object).Assembly.GetExportedTypes())
                .Where(candidate => candidate.IsVisible && !candidate.IsGenericType && !(candidate.IsAbstract && candidate.IsSealed))
                .ToArray();

            var generate = root.Q<Button>("generate");
            generate.SetEnabled(false);
            generate.clicked += Generate;

            var list = root.Q<ListView>("matches");
            list.selectionChanged += _ =>
            {
                type = (list.selectedItem as TypeEntry)?.Type;
                generate.SetEnabled(type != null);
                generate.text = type == null ? "Generate" : $"Generate {type.Name} Types";
            };

            root.Q<ToolbarSearchField>("search").RegisterValueChangedCallback(change =>
            {
                Matches.Clear();
                if (change.newValue.Length > 1)
                    Matches.AddRange(all
                        .Where(candidate => candidate.Name.Contains(change.newValue, StringComparison.OrdinalIgnoreCase))
                        .OrderBy(candidate => candidate.Name.Length)
                        .ThenBy(candidate => candidate.FullName)
                        .Take(100)
                        .Select(candidate => new TypeEntry { FullName = candidate.FullName, Type = candidate }));
                list.ClearSelection();
                list.RefreshItems();
            });

            root.Q<Button>("browse").clicked += () =>
            {
                string picked = EditorUtility.OpenFolderPanel("Folder for generated types", folder, "");
                if (picked.StartsWith(Application.dataPath))
                    folder = "Assets" + picked.Substring(Application.dataPath.Length);
            };
        }

        private void Generate()
        {
            string name = type.Name;
            string value = type.FullName.Replace('+', '.');
            string directory = Path.Combine(folder, name);
            Directory.CreateDirectory(directory);

            Write(Parts.Variable, directory, $"{name}VariableSO",
                $"[CreateAssetMenu(menuName = Config.VariableMenu + \"{name}\")]",
                $"public class {name}VariableSO : VariableSO<{value}> {{ }}",
                "",
                "[System.Serializable]",
                $"public class {name}Reference : VariableReference<{value}>",
                "{",
                $"    public {name}Reference() {{ }}",
                "",
                $"    public {name}Reference({value} value) : base(value) {{ }}",
                "}");

            Write(Parts.VariableListener, directory, $"VariableListener{name}",
                $"[AddComponentMenu(Config.VariableListenerMenu + \"Variable Listener {name}\")]",
                $"public class VariableListener{name} : VariableListener<{value}> {{ }}");

            Write(Parts.Event, directory, $"{name}Event",
                $"[CreateAssetMenu(menuName = Config.EventMenu + \"{name}\")]",
                $"public class {name}Event : GameEvent<{value}> {{ }}");

            Write(Parts.EventListener, directory, $"EventListener{name}",
                $"[AddComponentMenu(Config.EventListenerMenu + \"Event Listener {name}\")]",
                $"public class EventListener{name} : EventListener<{value}> {{ }}");

            Write(Parts.List, directory, $"{name}ListSO",
                $"[CreateAssetMenu(menuName = Config.VariableListMenu + \"{name}\")]",
                $"public class {name}ListSO : VariableListSO<{value}> {{ }}");

            AssetDatabase.Refresh();
        }

        private void Write(Parts part, string directory, string className, params string[] lines)
        {
            if (!parts.HasFlag(part)) return;

            string path = Path.Combine(directory, className + ".cs");
            if (File.Exists(path))
            {
                Debug.LogWarning($"Ripple: {path} already exists and was left untouched.");
                return;
            }

            bool wrapped = !string.IsNullOrEmpty(@namespace);
            var body = lines.Select(line => wrapped && line.Length > 0 ? "    " + line : line);
            var file = new[] { "using Ripple;", "using UnityEngine;", "" }.Concat(wrapped
                ? body.Prepend("{").Prepend($"namespace {@namespace}").Append("}")
                : body);
            File.WriteAllText(path, string.Join("\n", file) + "\n");
        }
    }
}
