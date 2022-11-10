using System;
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

        private class ValueType { }
    }
}