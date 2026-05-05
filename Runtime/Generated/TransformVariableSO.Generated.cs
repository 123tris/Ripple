using UnityEngine;
using UnityEngine.Scripting;

namespace Ripple
{
    [Preserve]
    [RippleData("Variables")]
    [CreateAssetMenu(menuName = "Ripple/Variables/Transform", fileName = "TransformVariable")]
    public sealed class TransformVariableSO : VariableSO<Transform>, IDisplayableVariable
    {
        public string ToDisplayString(string format) => CurrentValue != null ? CurrentValue.name : "<null>";
    }
}
