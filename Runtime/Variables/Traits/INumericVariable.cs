using System;

namespace Ripple
{
    public interface INumericVariable
    {
        double AsDouble { get; }
        void SetFromDouble(double value);
        Type ValueType { get; }
        IDisposable SubscribeNumeric(Action<double> handler);
    }
}
