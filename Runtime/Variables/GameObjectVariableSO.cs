using UnityEngine;

namespace Ripple
{
    [CreateAssetMenu(menuName = Config.VariableMenu + "GameObject")]
    public class GameObjectVariableSO : VariableSO<GameObject> {}

    internal static class Config
    {
        public const string VariableMenu = "Variables/";
        public const string VariableListMenu = "VariableLists/";
        public const string EventMenu = "Events/";

        public const string EventListenerMenu = "Ripple/Event Listener/";
        public const string VariableListenerMenu = "Ripple/Variable Listener/";
    }
}