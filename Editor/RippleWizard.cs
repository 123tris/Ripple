using System;
using System.Linq;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine.TextCore.Text;
using UnityEngine;

public class RippleWizard : OdinMenuEditorWindow
{
    [MenuItem("Tools/Open Ripple Wizard")]
    static void OpenWindow() => GetWindow<RippleWizard>();

    private static readonly Type[] typesToDisplay = TypeCache.GetTypesWithAttribute<RippleData>()
            .OrderBy(a => a.Name)
            .ToArray();

    protected override OdinMenuTree BuildMenuTree()
    {
        OdinMenuTree tree = new();

        var customMenuStyle = new OdinMenuStyle()
        {
            Height = 22,
            Offset = 15.00f,
            IndentAmount = 10.00f,
            IconSize = 16.00f,
            IconOffset = 0.00f,
            NotSelectedIconAlpha = 0.89f,
            IconPadding = 0.00f,
            TriangleSize = 16.00f,
            TrianglePadding = 0.00f,
            AlignTriangleLeft = true,
            Borders = true,
            BorderPadding = 0.00f,
            BorderAlpha = 0.32f,
            SelectedColorDarkSkin = new Color(0.243f, 0.373f, 0.588f, 1.000f),
            SelectedColorLightSkin = new Color(0.243f, 0.490f, 0.900f, 1.000f)
        };

        tree.DefaultMenuStyle = customMenuStyle;

        foreach (var type in typesToDisplay)
        {
            string path = type.Name;
            //if (type.IsSubclassOfRawGeneric(typeof(GameEvent<>)))
            //    path = path.Insert(0, "Events/");
            tree.AddAllAssetsAtPath(path, "Assets/", type, true, true);
        }

        tree.EnumerateTree()
            .AddThumbnailIcons(true);
            //.SortMenuItemsByName();

        return tree;
    }

    protected override void OnBeginDrawEditors() //TODO
    {
        var selected = this.MenuTree.Selection.FirstOrDefault();
        var toolbarHeight = this.MenuTree.Config.SearchToolbarHeight;

        SirenixEditorGUI.BeginHorizontalToolbar(toolbarHeight);
        {
            //if (selected != null)
            //{
            //    GUILayout.Label(selected.Name);
            //}

            if (SirenixEditorGUI.ToolbarButton(new GUIContent("This window is still work-in-progress, at the moment you can use it to make changes to all of your scriptable objects from Ripple")))
            {
                Debug.Log("WIP");
            }
        }
        SirenixEditorGUI.EndHorizontalToolbar();
    }
}