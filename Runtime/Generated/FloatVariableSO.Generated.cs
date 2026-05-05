using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace Ripple
{
    [Preserve]
    [RippleData("Variables")]
    [CreateAssetMenu(menuName = "Ripple/Variables/Float", fileName = "FloatVariable")]
    public sealed class FloatVariableSO : VariableSO<float>, INumericVariable, IComparableVariable, IDisplayableVariable
    {
        public double AsDouble => CurrentValue;
        public void SetFromDouble(double value) => SetCurrentValue((float)value);
        public Type ValueType => typeof(float);
        public IDisposable SubscribeNumeric(Action<double> handler) => Subscribe(v => handler(v));

        public int CompareTo(object other) =>
            other is INumericVariable n ? CurrentValue.CompareTo((float)n.AsDouble) : 0;

        public string ToDisplayString(string format) =>
            string.IsNullOrEmpty(format) ? CurrentValue.ToString() : CurrentValue.ToString(format);

        public void Add(float delta) => SetCurrentValue(CurrentValue + delta);
    }
}
