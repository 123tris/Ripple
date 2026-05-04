using UnityEngine;

namespace Ripple
{
    [RippleData]
    [CreateAssetMenu(menuName = Config.VariableMenu + "Float")]
    public class FloatVariableSO : NumericalVariable<float>, INumericVariable
    {
        protected override float Clamp(float value) => System.Math.Clamp(value, min, max);
        public override void Add(float value)
        {
            SetCurrentValue(CurrentValue + value);
        }

        public float GetNumericValue() => CurrentValue;

        public void SetNumericValue(float value, NumericWriteMode writeMode = NumericWriteMode.RoundToNearest, UnityEngine.Object invoker = null)
        {
            SetCurrentValue(value, invoker);
        }
    }
}