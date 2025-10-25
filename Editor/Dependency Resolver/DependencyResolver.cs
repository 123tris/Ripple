using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public static class DependencyResolver
{
    static DependencyResolver()
    {
        #if !ODIN_INSPECTOR
        Debug.LogError("Odin Inspector is not installed in this project. If you want to be able to use Ripple you will have to install Odin Inspector");
        #endif
        #if !ULTEVENTS
        Debug.LogError("Ultevents is not installed in this project. If you want to be able to use Ripple you will have to install Ultevents");
        #endif
    }
}
