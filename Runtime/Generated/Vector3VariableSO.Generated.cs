using UnityEngine;
using UnityEngine.Scripting;

namespace Ripple
{
    [Preserve]
    [RippleData("Variables")]
    [CreateAssetMenu(menuName = "Ripple/Variables/Vector3", fileName = "Vector3Variable")]
    public sealed class Vector3VariableSO : VariableSO<Vector3>, IDisplayableVariable
    {
        public string ToDisplayString(string format) => CurrentValue.ToString(format);
    }
}
