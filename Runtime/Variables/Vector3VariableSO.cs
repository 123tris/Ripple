using UnityEngine;

namespace Ripple
{
    [RippleData]
    [CreateAssetMenu(menuName = Config.VariableMenu + "Vector3")]
    public class Vector3VariableSO : VariableSO<Vector3>, INumericalVariable<Vector3>
    {
        public void Add(Vector3 value)
        {
            CurrentValue += value;
        }
    }
}