using NUnit.Framework;

namespace Ripple.Tests
{
    public class DispatcherTests
    {
        [Test]
        public void Add_Then_Invoke_DeliversValue()
        {
            var d = new Dispatcher<int>();
            int captured = 0;
            d.Add(v => captured = v);
            d.Invoke(7);
            Assert.AreEqual(7, captured);
        }

        [Test]
        public void Remove_Stops_Delivery()
        {
            var d = new Dispatcher<int>();
            int captured = 0;
            System.Action<int> handler = v => captured = v;
            d.Add(handler);
            d.Remove(handler);
            d.Invoke(5);
            Assert.AreEqual(0, captured);
        }

        [Test]
        public void Reentrancy_AddDuringDispatch_DefersToNextInvoke()
        {
            var d = new Dispatcher<int>();
            int aCount = 0, bCount = 0;
            System.Action<int> b = _ => bCount++;
            System.Action<int> a = _ => { aCount++; d.Add(b); };
            d.Add(a);
            d.Invoke(1);
            Assert.AreEqual(1, aCount);
            Assert.AreEqual(0, bCount);
            d.Invoke(2);
            Assert.AreEqual(2, aCount);
            Assert.AreEqual(1, bCount);
        }

        [Test]
        public void Reentrancy_RemoveDuringDispatch_StopsAtNextInvoke()
        {
            var d = new Dispatcher<int>();
            int count = 0;
            System.Action<int> handler = null;
            handler = _ => { count++; d.Remove(handler); };
            d.Add(handler);
            d.Invoke(1);
            d.Invoke(2);
            Assert.AreEqual(1, count);
        }

        [Test]
        public void Count_Reflects_Subscribers()
        {
            var d = new Dispatcher<int>();
            Assert.AreEqual(0, d.Count);
            System.Action<int> a = _ => { };
            d.Add(a);
            Assert.AreEqual(1, d.Count);
            d.Add(_ => { });
            Assert.AreEqual(2, d.Count);
            d.Remove(a);
            Assert.AreEqual(1, d.Count);
        }
    }
}
