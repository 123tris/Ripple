using System;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary> Value dropdown for scriptable objects.</summary>
[IncludeMyAttributes]
[ValueDropdown("@AssetDropdown.GetScriptableObjects($value)", AppendNextDrawer = true)]
[Required,AssetsOnly]
//[AttributeUsage(AttributeTargets.Field)]
public class AssetDropdown : Attribute
{
    public static T[] GetScriptableObjects<T>(T scriptableObject) where T : ScriptableObject
    {
        return Resources.FindObjectsOfTypeAll<T>();
    }
}
