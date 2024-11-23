using UnityEngine;

namespace Ripple
{
    [RippleData]
    [CreateAssetMenu(menuName = Config.VariableMenu + "Integer")]
    public class IntVariableSO : VariableSO<int>, INumericalVariable<int>
    {
        public void Add(int value)
        {
            CurrentValue += value;
        }
    }
}