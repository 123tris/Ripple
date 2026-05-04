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

    [MenuItem("Tools/Open Ripple Wizard")]
    static void OpenWindow() => GetWindow<RippleWizard>();

    private static Type[] TypesToDisplay => RippleTypeCache.GetCachedTypes();

    protected override OdinMenuTree BuildMenuTree()
    {
        var tree = new OdinMenuTree();

        tree.Config.SearchToolbarHeight = 22;

        tree.DefaultMenuStyle = new OdinMenuStyle
        {
            Height = 22, Offset = 15f, IndentAmount = 10f, IconSize = 16f,
            NotSelectedIconAlpha = 0.89f, TriangleSize = 16f, AlignTriangleLeft = true,
            Borders = true, BorderAlpha = 0.32f,
            SelectedColorDarkSkin = new Color(0.243f, 0.373f, 0.588f, 1f),
            SelectedColorLightSkin = new Color(0.243f, 0.490f, 0.900f, 1f)
        };

        string[] guids = GetOrCacheGuids();
        var validTypes = new HashSet<Type>(TypesToDisplay);

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            Type assetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);

            if (assetType != null && validTypes.Contains(assetType))
            {
                string assetName = Path.GetFileNameWithoutExtension(assetPath);
                string menuPath = BuildMenuPath(assetPath, assetType, assetName);
                tree.AddAssetAtPath(menuPath, assetPath);
            }
        }

        tree.EnumerateTree().AddThumbnailIcons(true);
        return tree;
    }

    private static string[] GetOrCacheGuids()
    {
        if (_cachedGuids != null)
            return _cachedGuids;

        var types = TypesToDisplay;
        if (types.Length == 0)
        {
            _cachedGuids = Array.Empty<string>();
            return _cachedGuids;
        }

        // Batch FindAssets calls per type to avoid the >1000 char filter bug
        var allGuids = new HashSet<string>();
        foreach (var type in types)
        {
            var guids = AssetDatabase.FindAssets("t:" + type.Name, new[] { "Assets" });
            foreach (var guid in guids)
                allGuids.Add(guid);
        }

        _cachedGuids = allGuids.ToArray();
        return _cachedGuids;
    }

    private static string BuildMenuPath(string assetPath, Type assetType, string assetName)
    {
        var asset = AssetDatabase.LoadAssetAtPath<GameEvent>(assetPath);
        if (asset != null && asset.Group != null)
            return asset.Group.DisplayName + "/" + assetName;
        return assetType.Name + "/" + assetName;
    }

    public void RefreshAssets()
    {
        _cachedGuids = null;
        ForceMenuTreeRebuild();
    }

    protected override void OnBeginDrawEditors()
    {
        var toolbarHeight = MenuTree.Config.SearchToolbarHeight;

        SirenixEditorGUI.BeginHorizontalToolbar(toolbarHeight);
        GUILayout.Label("Assets: " + (_cachedGuids?.Length.ToString() ?? "—"));
        GUILayout.FlexibleSpace();
        if (SirenixEditorGUI.ToolbarButton(new GUIContent(EditorIcons.Refresh.Raw, "Refresh Asset List")))
            RefreshAssets();
        SirenixEditorGUI.EndHorizontalToolbar();
    }

    private class RippleAssetWatcher : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(
            string[] importedAssets, string[] deletedAssets,
            string[] movedAssets, string[] movedFromAssetPaths)
        {
            bool anyScriptableObject =
                importedAssets.Concat(deletedAssets).Concat(movedAssets)
                    .Any(p => p.EndsWith(".asset", StringComparison.OrdinalIgnoreCase));

            if (!anyScriptableObject) return;

            _cachedGuids = null;
            var window = Resources.FindObjectsOfTypeAll<RippleWizard>().FirstOrDefault();
            window?.ForceMenuTreeRebuild();
        }
    }
}
