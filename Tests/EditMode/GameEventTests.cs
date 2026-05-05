using NUnit.Framework;
using UnityEngine;

namespace Ripple.Tests
{
    public class GameEventTests
    {
        [Test]
        public void Subscribe_Then_Invoke_DeliversValue()
        {
            var e = ScriptableObject.CreateInstance<IntEvent>();
            int captured = 0;
            using (e.Subscribe(v => captured = v))
            {
                e.Invoke(42);
                Assert.AreEqual(42, captured);
            }
            Object.DestroyImmediate(e);
        }

        [Test]
        public void Dispose_Unsubscribes()
        {
            var e = ScriptableObject.CreateInstance<IntEvent>();
            int count = 0;
            var sub = e.Subscribe(_ => count++);
            e.Invoke(1);
            sub.Dispose();
            e.Invoke(2);
            Assert.AreEqual(1, count);
            Object.DestroyImmediate(e);
        }

        [Test]
        public void VoidGameEvent_Invokes_With_Unit()
        {
            var e = ScriptableObject.CreateInstance<VoidGameEvent>();
            int count = 0;
            using (e.Subscribe(_ => count++))
            {
                e.Invoke();
            }
            Assert.AreEqual(1, count);
            Object.DestroyImmediate(e);
        }
    }
}
