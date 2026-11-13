using UnityEngine;

namespace Ripple
{
    [CreateAssetMenu(menuName = Config.VariableMenu + "Vector3")]
    public class Vector3VariableSO : NumericalVariable<Vector3>
    {
        protected override Vector3 Clamp(Vector3 value) => Vector3.Min(Vector3.Max(value, min), max);

        public override void Add(Vector3 value) => SetCurrentValue(CurrentValue + value);
    }
}
