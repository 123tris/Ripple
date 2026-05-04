using System;
using UnityEngine;

namespace Ripple
{
    [RippleData]
    [CreateAssetMenu(menuName = Config.VariableMenu + "Vector2")]
    public class Vector2VariableSO : NumericalVariable<Vector2>
    {
        protected override Vector2 Clamp(Vector2 value)
        {
            value.x = Math.Clamp(value.x, min.Value.x, max.Value.x);
            value.y = Math.Clamp(value.y, min.Value.y, max.Value.y);
            return value;
        }

        public override void Add(Vector2 value)
        {
            SetCurrentValue(CurrentValue + value);
        }
    }
}
