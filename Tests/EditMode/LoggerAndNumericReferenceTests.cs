using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace Ripple.Tests
{
    public class LoggerAndNumericReferenceTests
    {
        [SetUp]
        public void SetUp()
        {
            Logger.ResetLogs();
        }

        [Test]
        public void Logger_AssignsSequenceWithinFrame()
        {
            Logger.Log("first", null);
            Logger.Log("second", null);

            var logs = Logger.GetLogs();
            Assert.That(logs.Count, Is.EqualTo(2));
            Assert.That(logs[0].sequenceInFrame, Is.EqualTo(0));
            Assert.That(logs[1].sequenceInFrame, Is.EqualTo(1));
        }

        [Test]
        public void NumericVariableReference_ReadsAndWritesIntVariable()
        {
            var variable = ScriptableObject.CreateInstance<IntVariableSO>();
            variable.SetCurrentValue(10);

            var reference = new NumericVariableReference();
            SetPrivateField(reference, "_variable", variable);
            SetPrivateField(reference, "useConstant", false);

            Assert.That(reference.GetValue(), Is.EqualTo(10f));

            reference.SetValue(12.6f);
            Assert.That(variable.CurrentValue, Is.EqualTo(13));
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            Assert.That(field, Is.Not.Null, $"Field '{fieldName}' not found.");
            field.SetValue(target, value);
        }
    }
}
