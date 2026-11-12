using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Ripple.Tests
{
    public class VariableTests
    {
        private readonly List<Object> created = new();

        [TearDown]
        public void TearDown()
        {
            foreach (Object asset in created)
                Object.DestroyImmediate(asset);
            created.Clear();
        }

        private T Create<T>() where T : ScriptableObject
        {
            T asset = ScriptableObject.CreateInstance<T>();
            created.Add(asset);
            return asset;
        }

        [Test]
        public void SetCurrentValue_Clamped_StaysInRange()
        {
            var health = Create<FloatVariableSO>();
            health._isClamped = true;
            health.min = new FloatReference(0f);
            health.max = new FloatReference(10f);

            health.CurrentValue = 25f;
            Assert.AreEqual(10f, health.CurrentValue);

            health.Add(-40f);
            Assert.AreEqual(0f, health.CurrentValue);
        }

        [Test]
        public void SetCurrentValue_NotifiesBindings()
        {
            var score = Create<IntVariableSO>();
            string changed = null;
            score.propertyChanged += (_, args) => changed = args.propertyName;

            score.CurrentValue = 3;

            Assert.AreEqual(nameof(score.CurrentValue), changed);
        }

        [Test]
        public void ResetValue_ReturnsToInitialValue()
        {
            var score = Create<IntVariableSO>();
            score.CurrentValue = 7;

            score.ResetValue();

            Assert.AreEqual(0, score.CurrentValue);
        }

        [Test]
        public void ResetAll_ResetsVariablesAndLists()
        {
            var score = Create<IntVariableSO>();
            var speed = Create<FloatVariableSO>();
            var numbers = Create<IntListSO>();
            score.CurrentValue = 7;
            speed.CurrentValue = 2.5f;
            numbers.Add(4);

            BaseVariable.ResetAll();

            Assert.AreEqual(0, score.CurrentValue);
            Assert.AreEqual(0f, speed.CurrentValue);
            Assert.AreEqual(0, numbers.Count);
        }

        [Test]
        public void List_Resize_PadsAndTruncates()
        {
            var numbers = Create<IntListSO>();
            numbers.AddRange(new[] { 1, 2, 3 });

            numbers.Resize(5);
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 0, 0 }, numbers);

            numbers.Resize(2);
            CollectionAssert.AreEqual(new[] { 1, 2 }, numbers);
        }

        [Test]
        public void List_RaisesAddedAndRemoved()
        {
            var numbers = Create<IntListSO>();
            var added = new List<int>();
            var removed = new List<int>();
            numbers.OnItemAdded += added.Add;
            numbers.OnItemRemoved += removed.Add;

            numbers.AddRange(new[] { 1, 2 });
            numbers.Insert(0, 9);
            numbers.Clear();

            CollectionAssert.AreEqual(new[] { 1, 2, 9 }, added);
            CollectionAssert.AreEquivalent(new[] { 9, 1, 2 }, removed);
        }

        [Test]
        public void Signal_InvokesCodeListeners()
        {
            var signal = new Signal<int>();
            int received = 0;
            signal += value => received = value;

            signal.Invoke(5);

            Assert.AreEqual(5, received);
        }
    }
}
