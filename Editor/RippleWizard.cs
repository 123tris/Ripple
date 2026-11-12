using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ripple;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

public class RippleWizard : OdinMenuEditorWindow
{
    private static string[] _cachedGuids;

    [MenuItem("Tools/Ripple/Wizard")]
    private static void OpenWindow() => GetWindow<RippleWizard>();

    protected override OdinMenuTree BuildMenuTree()
    {
        OdinMenuTree tree = new();

        tree.DefaultMenuStyle = new OdinMenuStyle
        {
            Height = 22, Offset = 15.00f, IndentAmount = 10.00f, IconSize = 16.00f, IconOffset = 0.00f,
            NotSelectedIconAlpha = 0.89f, IconPadding = 0.00f, TriangleSize = 16.00f, TrianglePadding = 0.00f,
            AlignTriangleLeft = true, Borders = true, BorderPadding = 0.00f, BorderAlpha = 0.32f,
            SelectedColorDarkSkin = new Color(0.243f, 0.373f, 0.588f, 1.000f),
            SelectedColorLightSkin = new Color(0.243f, 0.490f, 0.900f, 1.000f)
        };

        var types = new HashSet<Type>(TypeCache.GetTypesWithAttribute<RippleData>());
        _cachedGuids ??= FindAssets(types);

        foreach (string guid in _cachedGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            Type assetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
            if (types.Contains(assetType))
                tree.AddAssetAtPath(assetType.Name + "/" + Path.GetFileNameWithoutExtension(assetPath), assetPath);
        }

        tree.EnumerateTree().AddThumbnailIcons(true);

        return tree;
    }

    private static string[] FindAssets(IEnumerable<Type> types)
    {
        string filter = "t:" + string.Join(" t:", types.Select(type => type.Name));
        if (filter.Length > 1000)
            filter = "t:ScriptableObject";
        return AssetDatabase.FindAssets(filter, new[] { "Assets" });
    }

    protected override void OnBeginDrawEditors()
    {
        SirenixEditorGUI.BeginHorizontalToolbar(MenuTree.Config.SearchToolbarHeight);
        {
            GUILayout.Label("Assets: " + (_cachedGuids?.Length ?? 0));
            GUILayout.FlexibleSpace();

            if (SirenixEditorGUI.ToolbarButton(new GUIContent(EditorIcons.Refresh.Raw, "Refresh Asset List")))
            {
                _cachedGuids = null;
                ForceMenuTreeRebuild();
            }
        }
        SirenixEditorGUI.EndHorizontalToolbar();
    }
}
