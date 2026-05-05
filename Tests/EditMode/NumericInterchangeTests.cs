using NUnit.Framework;
using UnityEngine;

namespace Ripple.Tests
{
    public class NumericInterchangeTests
    {
        [Test]
        public void Float_Int_Double_AllImplement_INumericVariable()
        {
            var f = ScriptableObject.CreateInstance<FloatVariableSO>();
            var i = ScriptableObject.CreateInstance<IntVariableSO>();
            var d = ScriptableObject.CreateInstance<DoubleVariableSO>();

            Assert.IsInstanceOf<INumericVariable>(f);
            Assert.IsInstanceOf<INumericVariable>(i);
            Assert.IsInstanceOf<INumericVariable>(d);

            f.SetFromDouble(2.5);
            i.SetFromDouble(2.5);
            d.SetFromDouble(2.5);

            Assert.AreEqual(2.5f, ((INumericVariable)f).AsDouble, 1e-6);
            Assert.AreEqual(3, ((INumericVariable)i).AsDouble, 1e-6); // rounds
            Assert.AreEqual(2.5, ((INumericVariable)d).AsDouble, 1e-9);

            Object.DestroyImmediate(f); Object.DestroyImmediate(i); Object.DestroyImmediate(d);
        }
    }
}
