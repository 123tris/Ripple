using UnityEngine;

namespace Ripple
{
    [CreateAssetMenu(menuName = Config.VariableMenu + "Integer")]
    public class IntVariableSO : NumericalVariable<int>
    {
        protected override int Clamp(int value) => Mathf.Clamp(value, min, max);
        public override void Add(int value)
        {
            SetCurrentValue(CurrentValue + value);
        }
    }
}