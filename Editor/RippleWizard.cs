using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ripple;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class RippleWizard : OdinMenuEditorWindow
{
    private static string[] _cachedGuids;


    [MenuItem("Tools/Open Ripple Wizard")]
    static void OpenWindow() => GetWindow<RippleWizard>();

    private static Type[] TypesToDisplay => RippleTypeCache.GetCachedTypes();

    protected override OdinMenuTree BuildMenuTree()
    {
        OdinMenuTree tree = new();

        var customMenuStyle = new OdinMenuStyle()
        {
            Height = 22, Offset = 15.00f, IndentAmount = 10.00f, IconSize = 16.00f, IconOffset = 0.00f,
            NotSelectedIconAlpha = 0.89f, IconPadding = 0.00f, TriangleSize = 16.00f, TrianglePadding = 0.00f,
            AlignTriangleLeft = true, Borders = true, BorderPadding = 0.00f, BorderAlpha = 0.32f,
            SelectedColorDarkSkin = new Color(0.243f, 0.373f, 0.588f, 1.000f),
            SelectedColorLightSkin = new Color(0.243f, 0.490f, 0.900f, 1.000f)
        };
        tree.DefaultMenuStyle = customMenuStyle;

        string[] guids = GetOrCacheGuids();

        var types = TypesToDisplay;
        var validTypes = new HashSet<Type>(types);

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            Type assetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);

            if (assetType != null && validTypes.Contains(assetType))
            {
                string menuPath = assetType.Name + "/" + Path.GetFileNameWithoutExtension(assetPath);
                tree.AddAssetAtPath(menuPath, assetPath);
            }
        }

        tree.EnumerateTree().AddThumbnailIcons(true);

        return tree;
    }

    private static string[] GetOrCacheGuids()
    {
        if (_cachedGuids != null)
        {
            return _cachedGuids;
        }

        var types = TypesToDisplay;

        string filter = "t:" + string.Join(" t:", types.Select(t => t.Name));

        if (filter.Length > 1000 || string.IsNullOrWhiteSpace(filter))
        {
            filter = "t:ScriptableObject";
        }

        _cachedGuids = AssetDatabase.FindAssets(filter, new[] { "Assets" });

        return _cachedGuids;
    }

    private void RefreshAssets()
    {
        _cachedGuids = null;
        ForceMenuTreeRebuild();
    }

    protected override void OnBeginDrawEditors()
    {
        var toolbarHeight = this.MenuTree.Config.SearchToolbarHeight;

        SirenixEditorGUI.BeginHorizontalToolbar(toolbarHeight);
        {
            GUILayout.Label("Cached Assets: " + (_cachedGuids?.Length.ToString() ?? "0"));

            GUILayout.FlexibleSpace();
            
            GUIContent refreshButtonContent = new(EditorIcons.Refresh.Raw, "Refresh Asset List");

            if (SirenixEditorGUI.ToolbarButton(refreshButtonContent))
            {
                RefreshAssets();
            }
        }
        SirenixEditorGUI.EndHorizontalToolbar();
    }
}