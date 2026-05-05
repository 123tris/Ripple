using UnityEngine;
using UnityEngine.Scripting;

namespace Ripple
{
    [Preserve]
    [RippleData("Variables")]
    [CreateAssetMenu(menuName = "Ripple/Variables/Bool", fileName = "BoolVariable")]
    public sealed class BoolVariableSO : VariableSO<bool>, IDisplayableVariable
    {
        public string ToDisplayString(string format) => CurrentValue.ToString();
    }
}
