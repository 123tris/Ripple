using UnityEngine;
using UnityEngine.Scripting;

namespace Ripple
{
    [Preserve]
    [RippleData("Variables")]
    [CreateAssetMenu(menuName = "Ripple/Variables/String", fileName = "StringVariable")]
    public sealed class StringVariableSO : VariableSO<string>, IDisplayableVariable
    {
        public string ToDisplayString(string format) => CurrentValue ?? string.Empty;
    }
}
