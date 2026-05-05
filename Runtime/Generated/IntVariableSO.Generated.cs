using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace Ripple
{
    [Preserve]
    [RippleData("Variables")]
    [CreateAssetMenu(menuName = "Ripple/Variables/Int", fileName = "IntVariable")]
    public sealed class IntVariableSO : VariableSO<int>, INumericVariable, IComparableVariable, IDisplayableVariable
    {
        public double AsDouble => CurrentValue;
        public void SetFromDouble(double value) => SetCurrentValue((int)Math.Round(value));
        public Type ValueType => typeof(int);
        public IDisposable SubscribeNumeric(Action<double> handler) => Subscribe(v => handler((double)v));

        public int CompareTo(object other) =>
            other is INumericVariable n ? CurrentValue.CompareTo((int)n.AsDouble) : 0;

        public string ToDisplayString(string format) =>
            string.IsNullOrEmpty(format) ? CurrentValue.ToString() : CurrentValue.ToString(format);

        public void Add(int delta) => SetCurrentValue(CurrentValue + delta);
    }
}
