using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace Ripple
{
    [Preserve]
    [RippleData("Variables")]
    [CreateAssetMenu(menuName = "Ripple/Variables/Double", fileName = "DoubleVariable")]
    public sealed class DoubleVariableSO : VariableSO<double>, INumericVariable, IComparableVariable, IDisplayableVariable
    {
        public double AsDouble => CurrentValue;
        public void SetFromDouble(double value) => SetCurrentValue(value);
        public Type ValueType => typeof(double);
        public IDisposable SubscribeNumeric(Action<double> handler) => Subscribe(handler);

        public int CompareTo(object other) =>
            other is INumericVariable n ? CurrentValue.CompareTo(n.AsDouble) : 0;

        public string ToDisplayString(string format) =>
            string.IsNullOrEmpty(format) ? CurrentValue.ToString() : CurrentValue.ToString(format);

        public void Add(double delta) => SetCurrentValue(CurrentValue + delta);
    }
}
