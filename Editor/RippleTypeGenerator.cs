using System;
using System.IO;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Ripple
{
    public class RippleTypeGenerator : OdinEditorWindow
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

        [ShowInInspector, InfoBox("Generic types are not supported.", InfoMessageType.Error, "@type != null && type.IsGenericType")]
        private Type type;

        [SerializeField, FolderPath] private string folder = "Assets/Ripple";

        [SerializeField] private string @namespace;

        [SerializeField, EnumToggleButtons, HideLabel]
        private Parts parts = Parts.Variable | Parts.VariableListener | Parts.Event | Parts.EventListener | Parts.List;

        [MenuItem("Tools/Ripple/Type Generator")]
        private static void OpenWindow() => GetWindow<RippleTypeGenerator>("Ripple Type Generator");

        protected override void OnEnable()
        {
            base.OnEnable();
            if (string.IsNullOrEmpty(@namespace))
                @namespace = EditorSettings.projectGenerationRootNamespace;
        }

        [Button(ButtonSizes.Large), EnableIf("@type != null && !type.IsGenericType")]
        private void Generate()
        {
            string name = type.Name;
            string value = type.FullName.Replace('+', '.');
            string directory = Path.Combine(folder, name);
            Directory.CreateDirectory(directory);

            Write(Parts.Variable, directory, $"{name}VariableSO",
                "[RippleData]",
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
                "[RippleData]",
                $"[CreateAssetMenu(menuName = Config.EventMenu + \"{name}\")]",
                $"public class {name}Event : GameEvent<{value}> {{ }}");

            Write(Parts.EventListener, directory, $"EventListener{name}",
                $"[AddComponentMenu(Config.EventListenerMenu + \"Event Listener {name}\")]",
                $"public class EventListener{name} : EventListener<{value}> {{ }}");

            Write(Parts.List, directory, $"{name}ListSO",
                "[RippleData]",
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
