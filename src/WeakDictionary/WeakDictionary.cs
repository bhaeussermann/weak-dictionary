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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace BernhardHaus.Collections.WeakDictionary
{
    public class WeakDictionary<TKey, TValue> : IDictionary<TKey, TValue>
        where TValue : class
    {
        private readonly Dictionary<TKey, WeakReference> internalDictionary = new Dictionary<TKey, WeakReference>();
        private readonly ConditionalWeakTable<TValue, Finalizer> conditionalWeakTable = new ConditionalWeakTable<TValue, Finalizer>();

        public TValue this[TKey key]
        {
            get => (TValue)this.internalDictionary[key].Target;
            set
            {
                Remove(key);
                Add(key, value);
            }
        }

        public ICollection<TKey> Keys => this.internalDictionary.Keys;

        public ICollection<TValue> Values => this.internalDictionary.Values.Select(r => (TValue)r.Target).ToArray();

        public int Count => this.internalDictionary.Count;

        public bool IsReadOnly => false;

        public void Add(TKey key, TValue value)
        {
            this.internalDictionary.Add(key, new WeakReference(value));
            var finalizer = new Finalizer(key);
            finalizer.ValueFinalized += k => Remove(k);
            this.conditionalWeakTable.Add(value, finalizer);
        }

        public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);

        public void Clear() => this.internalDictionary.Clear();

        public bool Contains(KeyValuePair<TKey, TValue> item) => ((IDictionary<TKey, TValue>)this.internalDictionary).Contains(item);

        public bool ContainsKey(TKey key) => this.internalDictionary.ContainsKey(key);

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => ((IDictionary<TKey, TValue>)this.internalDictionary).CopyTo(array, arrayIndex);

        public bool Remove(TKey key) => this.internalDictionary.Remove(key);

        public bool Remove(KeyValuePair<TKey, TValue> item) => ((IDictionary<TKey, TValue>)this.internalDictionary).Remove(item);

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (this.internalDictionary.TryGetValue(key, out var valueReference))
            {
                value = (TValue)valueReference.Target;
                return true;
            }

            value = default;
            return false;
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return this.internalDictionary.Select(kvp => new KeyValuePair<TKey, TValue>(kvp.Key, (TValue)kvp.Value.Target)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private sealed class Finalizer
        {
            private readonly TKey valueKey;

            public Finalizer(TKey valueKey)
            {
                this.valueKey = valueKey;
            }

            ~Finalizer()
            {
                ValueFinalized?.Invoke(this.valueKey);
            }

            public event ValueFinalizedDelegate ValueFinalized;
        }

        private delegate void ValueFinalizedDelegate(TKey valueKey);
    }
}