using System;
using Sirenix.OdinInspector;
using UnityEngine;

[IncludeMyAttributes]
[ValueDropdown("@AssetDropdown.GetScriptableObjects($value)", AppendNextDrawer = true)]
[AssetsOnly]
public class AssetDropdown : Attribute
{
    public static T[] GetScriptableObjects<T>(T scriptableObject) where T : ScriptableObject
    {
        return Resources.FindObjectsOfTypeAll<T>();
    }
}
