using UnityEngine;

namespace Ripple
{
    [RippleData]
    [CreateAssetMenu(menuName = Config.VariableMenu + "Float")]
    public class FloatVariableSO : NumericalVariable<float>
    {
        protected override float Clamp(float value) => System.Math.Clamp(value, min, max);
    }
}