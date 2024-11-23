using UnityEngine;

namespace Ripple
{
    [RippleData]
    [CreateAssetMenu(menuName = Config.VariableMenu + "Float")]
    public class FloatVariableSO : VariableSO<float>, INumericalVariable<float>
    {
        public void Add(float value)
        {
            CurrentValue += value;
        }
    }
}