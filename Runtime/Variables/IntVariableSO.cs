using UnityEngine;

namespace Ripple
{
    [RippleData]
    [CreateAssetMenu(menuName = Config.VariableMenu + "Int")]
    public class IntVariableSO : NumericalVariable<int>, INumericVariable
    {
        protected override int Clamp(int value) => System.Math.Clamp(value, min, max);
        public override void Add(int value)
        {
            SetCurrentValue(CurrentValue + value);
        }

        public float GetNumericValue() => CurrentValue;

        public void SetNumericValue(float value, NumericWriteMode writeMode = NumericWriteMode.RoundToNearest, UnityEngine.Object invoker = null)
        {
            int convertedValue = writeMode switch
            {
                NumericWriteMode.Floor => Mathf.FloorToInt(value),
                NumericWriteMode.Ceil => Mathf.CeilToInt(value),
                NumericWriteMode.Truncate => (int)value,
                _ => Mathf.RoundToInt(value)
            };
            SetCurrentValue(convertedValue, invoker);
        }
    }
}