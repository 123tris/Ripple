using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Ripple
{
    [RippleData]
    [CreateAssetMenu(menuName = Config.VariableMenu + "Vector3")]
    public class Vector3VariableSO : NumericalVariable<Vector3>
    {
        protected override Vector3 Clamp(Vector3 value)
        {
            value.x = Math.Clamp(value.x, min.Value.x, max.Value.x);
            value.y = Math.Clamp(value.y, min.Value.y, max.Value.y);
            value.z = Math.Clamp(value.z, min.Value.z, max.Value.z);
            return value;
        }

        public override void Add(Vector3 value)
        {
            SetCurrentValue(CurrentValue + value);
        }
    }
}