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
        private readonly IDictionary<TKey, WeakReference> internalDictionary = new Dictionary<TKey, WeakReference>();
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

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return this.internalDictionary.TryGetValue(item.Key, out var valueReference) && valueReference.Target.Equals(item.Value);
        }

        public bool ContainsKey(TKey key) => this.internalDictionary.ContainsKey(key);

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            foreach (KeyValuePair<TKey, WeakReference> keyValuePair in this.internalDictionary)
            {
                array[arrayIndex++] = new KeyValuePair<TKey, TValue>(keyValuePair.Key, (TValue)keyValuePair.Value.Target);
            }
        }

        public bool Remove(TKey key) => this.internalDictionary.Remove(key);

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return this.internalDictionary.TryGetValue(item.Key, out var valueReference)
                && valueReference.Target.Equals(item.Value)
                && this.internalDictionary.Remove(item.Key);
        }

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