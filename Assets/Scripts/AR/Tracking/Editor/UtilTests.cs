using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Collections;

namespace AR.Tests
{
    class UtilTests
    {
        private static int SortAscending(int a, int b)
        {
            return a.CompareTo(b);
        }

        [Test]
        public void InsertionSort_TestSingle()
        {
            var a = new int[] { 1 };
            Utils.InsertionSort(a, SortAscending);
            Assert.True(a[0] == 1);
        }

        [Test]
        public void InsertionSort_TestSortAscending()
        {
            var a = new int[] { 2, 1, 4, 7 };
            Utils.InsertionSort(a, SortAscending);
            CollectionAssert.IsOrdered(a);
        }

        [Test]
        public void InsertionSort_SortedArrayShouldBeSortedAfterSorting()
        {
            var a = new int[] { 1, 2, 3, 4 };
            Utils.InsertionSort(a, SortAscending);
            CollectionAssert.IsOrdered(a);
        }

        [Test]
        public void InsertionSort_TestDuplicate()
        {
            var a = new int[] { 2, 1, 1 };
            Utils.InsertionSort(a, SortAscending);
            CollectionAssert.IsOrdered(a);
        }

        private class Item : IComparable<Item>
        {
            public int value;
            public int order;

            public Item(int v, int o)
            {
                this.value = v;
                this.order = o;
            }

            public int CompareTo(Item other)
            {
                return value.CompareTo(other.value);
            }
        }

        [Test]
        public void InsertionSort_SortShouldBeStable()
        {
            var a = new[] {
                new Item(1, 0),
                new Item(2, 1),
                new Item(1, 2)
            };
            Utils.InsertionSort(a);
            CollectionAssert.IsOrdered(a);

            for (int i = 1; i < a.Length; ++i)
            {
                if (a[i].value == a[i-1].value)
                {
                    Assert.True(a[i].order >= a[i - 1].order);
                }
            }
        }
    }
}
