using UnityEngine;
using UnityEngine.Scripting;

namespace Ripple
{
    [Preserve]
    [RippleData("Variables")]
    [CreateAssetMenu(menuName = "Ripple/Variables/Vector2", fileName = "Vector2Variable")]
    public sealed class Vector2VariableSO : VariableSO<Vector2>, IDisplayableVariable
    {
        public string ToDisplayString(string format) => CurrentValue.ToString(format);
    }
}
