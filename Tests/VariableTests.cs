using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
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
        public void LocalVariables_GiveEachObjectItsOwnCopy()
        {
            var health = Create<FloatVariableSO>();
            Transform first = CreateWithLocal(health);
            Transform second = CreateWithLocal(health);
            Transform shared = CreateChild(null);

            FloatVariableSO firstHealth = health.For(first);
            FloatVariableSO secondHealth = health.For(second);
            created.Add(firstHealth);
            created.Add(secondHealth);
            firstHealth.CurrentValue = 3f;

            Assert.AreEqual(3f, firstHealth.CurrentValue);
            Assert.AreEqual(0f, secondHealth.CurrentValue);
            Assert.AreEqual(0f, health.CurrentValue);
            Assert.AreSame(firstHealth, health.For(first));
            Assert.AreSame(health, health.For(shared));
        }

        [Test]
        public void LocalVariables_LeaveUnlistedVariablesShared()
        {
            var health = Create<FloatVariableSO>();
            var score = Create<IntVariableSO>();
            Transform child = CreateWithLocal(health);

            Assert.AreSame(score, score.For(child));
        }

        [Test]
        public void LocalVariables_CopiesDoNotPersist()
        {
            var health = Create<FloatVariableSO>();
            health.name = "RippleTestHealth";
            var serialized = new SerializedObject(health);
            serialized.FindProperty("_persist").boolValue = true;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            FloatVariableSO copy = health.For(CreateWithLocal(health));
            created.Add(copy);

            copy.CurrentValue = 3f;

            bool saved = PlayerPrefs.HasKey(copy.name);
            PlayerPrefs.DeleteKey(copy.name);
            Assert.IsFalse(saved);
        }

        [Test]
        public void List_IndexerRaisesRemovedThenAdded()
        {
            var numbers = Create<IntListSO>();
            numbers.AddRange(new[] { 1, 2 });
            var changes = new List<string>();
            numbers.OnItemAdded += item => changes.Add("+" + item);
            numbers.OnItemRemoved += item => changes.Add("-" + item);

            numbers[0] = 9;

            CollectionAssert.AreEqual(new[] { 9, 2 }, numbers);
            CollectionAssert.AreEqual(new[] { "-1", "+9" }, changes);
        }

        [Test]
        public void List_RecordsWhoChangedIt()
        {
            var numbers = Create<IntListSO>();

            numbers.Add(4);
            numbers.Remove(4);

            Assert.AreEqual(1, numbers.Invokers.Count);
            Assert.AreEqual(2, numbers.Invokers[0].Count);
            Assert.AreEqual("- 4", numbers.Invokers[0].LastValue);
            StringAssert.Contains(nameof(List_RecordsWhoChangedIt), numbers.Invokers[0].Caller);
        }

        [Test]
        public void Clamp_WithInvertedRange_DoesNotThrow()
        {
            var speed = Create<FloatVariableSO>();
            speed._isClamped = true;
            speed.min = new FloatReference(10f);
            speed.max = new FloatReference(0f);
            var lives = Create<IntVariableSO>();
            lives._isClamped = true;
            lives.min = new IntReference(3);
            lives.max = new IntReference(1);

            Assert.DoesNotThrow(() => speed.CurrentValue = 5f);
            Assert.DoesNotThrow(() => lives.CurrentValue = 2);
        }

        [Test]
        public void Vector2Variable_ClampsEachAxis()
        {
            var aim = Create<Vector2VariableSO>();
            aim._isClamped = true;
            aim.min = new Vector2Reference(Vector2.zero);
            aim.max = new Vector2Reference(Vector2.one);

            aim.CurrentValue = new Vector2(2f, -1f);

            Assert.AreEqual(new Vector2(1f, 0f), aim.CurrentValue);
        }

        [Test]
        public void PreviousValue_IsTheValueBeforeTheLastChange()
        {
            var template = Create<IntVariableSO>();
            var serialized = new SerializedObject(template);
            serialized.FindProperty("_initialValue").intValue = 5;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            var score = Object.Instantiate(template);
            created.Add(score);

            Assert.AreEqual(5, score.PreviousValue);
            score.CurrentValue = 7;
            score.CurrentValue = 9;

            Assert.AreEqual(7, score.PreviousValue);
        }

        private Transform CreateWithLocal(BaseVariable variable)
        {
            var root = new GameObject("Root", typeof(LocalVariables));
            created.Add(root);
            var serialized = new SerializedObject(root.GetComponent<LocalVariables>());
            SerializedProperty variables = serialized.FindProperty("variables");
            variables.arraySize = 1;
            variables.GetArrayElementAtIndex(0).objectReferenceValue = variable;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return CreateChild(root.transform);
        }

        private Transform CreateChild(Transform parent)
        {
            var child = new GameObject("Child");
            created.Add(child);
            child.transform.SetParent(parent);
            return child.transform;
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
