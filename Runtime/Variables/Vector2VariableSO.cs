using UnityEngine;

namespace Ripple
{
    [CreateAssetMenu(menuName = Config.VariableMenu + "Vector2")]
    public class Vector2VariableSO : NumericalVariable<Vector2>
    {
        protected override Vector2 Clamp(Vector2 value) => Vector2.Min(Vector2.Max(value, min), max);

        public override void Add(Vector2 value) => SetCurrentValue(CurrentValue + value);
    }
}
