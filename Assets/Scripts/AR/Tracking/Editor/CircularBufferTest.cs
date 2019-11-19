using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace AR.Tests
{
    class CircularBufferTest
    {
        [Test]
        public void TestEmpty()
        {
            CircularBuffer<int> buf = new CircularBuffer<int>(1);
            Assert.True(buf.Count == 0);
        }

        [Test]
        public void TestAdd()
        {
            CircularBuffer<int> buf = new CircularBuffer<int>(1);
            buf.Add(1);
            Assert.True(buf.Count == 1);
            Assert.True(buf[0] == 1);
        }

        [Test]
        public void TestOverflowAdd()
        {
            CircularBuffer<int> buf = new CircularBuffer<int>(1);
            buf.Add(1);
            buf.Add(2);
            Assert.True(buf.Count == 1);
            Assert.True(buf[0] == 2);
        }

        [Test]
        public void TestEnumerable()
        {
            CircularBuffer<int> buf = new CircularBuffer<int>(2);
            buf.Add(1);
            buf.Add(2);
            int[] expected = new int[] { 1, 2 };
            int i = 0;
            foreach (var e in buf)
            {
                Assert.True(e == expected[i++]);
            }
        }

        [Test]
        public void TestIndexer()
        {
            CircularBuffer<int> buf = new CircularBuffer<int>(2);
            buf.Add(1);
            buf.Add(2);
            int[] expected = new int[] { 1, 2 };
            for (int i = 0; i < buf.Count; ++i)
            {
                Assert.True(buf[i] == expected[i]);
            }
        }


        [Test]
        public void TestClear()
        {
            CircularBuffer<int> buf = new CircularBuffer<int>(2);
            buf.Add(1);
            buf.Add(2);
            buf.Clear();
            Assert.True(buf.Count == 0);
        }

        [Test]
        public void TestEnlargeCapacity()
        {
            CircularBuffer<int> buf = new CircularBuffer<int>(2);
            Assert.True(buf.Capacity == 2);

            buf.Add(1);
            buf.Add(2);

            buf.Capacity = 3;
            Assert.True(buf.Capacity == 3);
            Assert.True(buf.Count == 2);

            int[] exptected = new int[] { 1, 2 };
            for (int i = 0; i < buf.Count; ++i)
            {
                Assert.True(buf[i] == exptected[i]);
            }
        }

        [Test]
        public void TestPopException()
        {
            CircularBuffer<int> buf = new CircularBuffer<int>(1);
            Assert.Throws<InvalidOperationException>(delegate {
                buf.Pop();
            });
        }

        [Test]
        public void TestIndexerOutOfBound()
        {
            CircularBuffer<int> buf = new CircularBuffer<int>(1);
            Assert.Throws<ArgumentOutOfRangeException>(delegate {
#pragma warning disable 168
                var x = buf[0];
#pragma warning restore 168
            });
        }
    }
}
