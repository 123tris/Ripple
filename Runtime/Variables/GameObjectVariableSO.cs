using UnityEngine;

[CreateAssetMenu(menuName = Config.VariableMenu + "GameObject")]
public class GameObjectVariableSO : VariableSO<GameObject> {}

internal static class Config
{
    public const string VariableMenu = "Variables/";
    public const string EventMenu = "Events/";
}