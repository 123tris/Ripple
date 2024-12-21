using UnityEngine;

namespace Ripple
{
    [RippleData]
    [CreateAssetMenu(menuName = Config.VariableMenu + "Integer")]
    public class IntVariableSO : NumericalVariable<int>
    {
        protected override int Clamp(int value) => System.Math.Clamp(value, min, max);
    }
}