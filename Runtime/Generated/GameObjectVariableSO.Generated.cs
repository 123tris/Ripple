using UnityEngine;
using UnityEngine.Scripting;

namespace Ripple
{
    [Preserve]
    [RippleData("Variables")]
    [CreateAssetMenu(menuName = "Ripple/Variables/GameObject", fileName = "GameObjectVariable")]
    public sealed class GameObjectVariableSO : VariableSO<GameObject>, IDisplayableVariable
    {
        public string ToDisplayString(string format) => CurrentValue != null ? CurrentValue.name : "<null>";
    }
}
