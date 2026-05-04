using UnityEngine;

namespace Ripple
{
    /// <summary>
    /// Organisational container for grouping related events and variables in the Ripple Wizard.
    /// Assign to a GameEvent's _group field to display it under GroupName/AssetName in the tree.
    /// </summary>
    [CreateAssetMenu(menuName = "Ripple/Event Group")]
    public class EventGroupSO : ScriptableObject
    {
        [SerializeField] private string _displayName;

        public string DisplayName => string.IsNullOrWhiteSpace(_displayName) ? name : _displayName;
    }
}
