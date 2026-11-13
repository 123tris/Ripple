using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Ripple.Tests
{
    public class EventTests
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
        public void AnyEvent_ListenerHearsVoidAndTypedEvents()
        {
            var gameOver = Create<VoidEventSO>();
            var damaged = Create<FloatEvent>();
            int heard = 0;
            foreach (GameEvent gameEvent in new GameEvent[] { gameOver, damaged })
                gameEvent.AddListener(() => heard++);

            gameOver.Raise();
            damaged.Raise(3f);

            Assert.AreEqual(2, heard);
            Assert.IsTrue(gameOver.HasListeners);
            Assert.IsTrue(damaged.HasListeners);
        }

        [Test]
        public void AnyEvent_RemovedListenerHearsNothing()
        {
            GameEvent damaged = Create<FloatEvent>();
            int heard = 0;
            void Count() => heard++;
            damaged.AddListener(Count);
            damaged.RemoveListener(Count);

            damaged.Raise(3f);

            Assert.AreEqual(0, heard);
            Assert.IsFalse(damaged.HasListeners);
        }

        [Test]
        public void TypedEvent_StillTakesAnImplicitConversion()
        {
            var damaged = Create<FloatEvent>();
            float received = 0f;
            damaged.AddListener(value => received = value);

            damaged.Raise(3);

            Assert.AreEqual(3f, received);
        }

        [Test]
        public void AnyEvent_RaisePassesTheValueToAnEventOfThatType()
        {
            var damaged = Create<FloatEvent>();
            float received = 0f;
            damaged.AddListener(value => received = value);
            GameEvent any = damaged;

            any.Raise(3f);

            Assert.AreEqual(3f, received);
        }

        [Test]
        public void AnyEvent_RaiseDropsTheValueForAVoidEvent()
        {
            GameEvent gameOver = Create<VoidEventSO>();
            bool raised = false;
            gameOver.AddListener(() => raised = true);

            gameOver.Raise(3f);

            Assert.IsTrue(raised);
        }

        [Test]
        public void AnyEvent_RaiseWithAnotherTypeThrows()
        {
            GameEvent score = Create<IntEvent>();
            score.name = "Score";

            var exception = Assert.Throws<InvalidOperationException>(() => score.Raise(3f));

            StringAssert.Contains("Score (IntEvent)", exception.Message);
            StringAssert.Contains("Single", exception.Message);
        }

        [Test]
        public void Signal_RaisesVoidAndTypedEventsFromItsList()
        {
            var damaged = Create<FloatEvent>();
            var gameOver = Create<VoidEventSO>();
            var healed = Create<FloatEvent>();
            var serialized = new SerializedObject(damaged);
            SerializedProperty raise = serialized.FindProperty("response.raise");
            raise.arraySize = 2;
            raise.GetArrayElementAtIndex(0).objectReferenceValue = gameOver;
            raise.GetArrayElementAtIndex(1).objectReferenceValue = healed;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            bool over = false;
            float amount = 0f;
            gameOver.AddListener(() => over = true);
            healed.AddListener(value => amount = value);

            damaged.Raise(4f);

            Assert.IsTrue(over);
            Assert.AreEqual(4f, amount);
        }
    }
}
