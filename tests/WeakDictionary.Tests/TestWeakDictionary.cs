using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace BernhardHaus.Collections.WeakDictionary.Tests
{
    [TestFixture]
    internal class TestWeakDictionary
    {
        [Test]
        public void RemoveGarbageCollectedEntries()
        {
            var v1 = new ValueType();
            var v2 = new ValueType();
            var v3 = new ValueType();

            var dictionary = new WeakDictionary<int, ValueType>
            {
                { 1, v1 },
                { 2, v2 },
                { 3, v3 }
            };

            var weakReference = new WeakReference(v2);
            v2 = null;

            // Invoke garbage collection within a loop so as to avoid the call to be removed by compiler optimizations.
            for (int i = 0; i < 1; i++)
            {
                GC.Collect();
            }

            Assert.That(weakReference.IsAlive, Is.False);
            CollectionAssert.AreEquivalent(new int[] { 1, 3 }, dictionary.Keys, "Unexpected keys after garbage collection.");

            // Ensure these references stay alive and are not nulled by compiler optimizations.
            v1.ToString();
            v3.ToString();
        }

        [Test]
        public void ContainsKeyValuePair()
        {
            var dictionary = new WeakDictionary<int, string>
            {
                { 1, "One" },
                { 2, "Two" }
            };

            Assert.That(dictionary.Contains(new KeyValuePair<int, string>(1, "One")), Is.True);
            Assert.That(dictionary, Does.Not.Contain(new KeyValuePair<int, string>(1, "Two")));
            Assert.That(dictionary, Does.Not.Contain(new KeyValuePair<int, string>(3, "Two")));
        }

        [Test]
        public void RemoveKeyValuePair()
        {
            var dictionary = new WeakDictionary<int, string>
            {
                { 1, "One" },
                { 2, "Two" }
            };

            Assert.That(dictionary.Remove(new KeyValuePair<int, string>(1, "One")), Is.True);
            Assert.That(dictionary.AsEnumerable(), Is.EqualTo(new[] { new KeyValuePair<int, string>(2, "Two") }));

            Assert.That(dictionary.Remove(new KeyValuePair<int, string>(2, "Three")), Is.False);
            Assert.That(dictionary.AsEnumerable(), Is.EqualTo(new[] { new KeyValuePair<int, string>(2, "Two") }));
        }

        [Test]
        public void CopyTo()
        {
            var dictionary = new WeakDictionary<int, string>
            {
                { 1, "One" },
                { 2, "Two" }
            };
            var array = new KeyValuePair<int, string>[2];
            dictionary.CopyTo(array, 0);

            Assert.That(array, Is.EquivalentTo(new[]
            {
                new KeyValuePair<int, string>(1, "One"),
                new KeyValuePair<int, string>(2, "Two")
            }));
        }

        private class ValueType { }
    }
}