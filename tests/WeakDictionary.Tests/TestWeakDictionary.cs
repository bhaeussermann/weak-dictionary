// 
// Copyright(c) 2022 Bernhard Häussermann
// 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without 
// modification, are permitted provided that the following conditions 
// are met:
// 
// * Redistributions of source code must retain the above copyright notice,
//   this list of conditions and the following disclaimer. 
// 
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution. 
// 
// * Neither the name of Bernhard Häussermann nor the names of its 
//   contributors may be used to endorse or promote products derived from this
//   software without specific prior written permission. 
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF 
// THE POSSIBILITY OF SUCH DAMAGE.
// 

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