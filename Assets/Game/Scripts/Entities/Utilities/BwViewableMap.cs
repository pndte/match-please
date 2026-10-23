using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.Entities
{
    public sealed class BwViewableMap<K, V> : IViewableMap<K, V>, IReadonlyViewableMap<K, V>
    {
        public ISource<MapEvent<K, V>> Change => _map.Change;
        public int Count => _map.Count;
        public bool IsReadOnly => _map.IsReadOnly;
        public ICollection<K> Keys => _map.Keys;
        public ICollection<V> Values => _map.Values;
        IEnumerable<K> IReadOnlyDictionary<K, V>.Keys => _map.Keys;
        IEnumerable<V> IReadOnlyDictionary<K, V>.Values => _map.Values;

        private readonly ViewableMap<K, V> _map = new();

        public V this[K key]
        {
            get => _map[key];
            set => _map[key] = value;
        }

        public void Advise(Lifetime lifetime, Action<MapEvent<K, V>> handler) =>
            _map.Advise(lifetime, handler);

        public void Add(K key, V value) =>
            _map.Add(key, value);

        public void Add(KeyValuePair<K, V> item) =>
            _map.Add(item);

        public bool Remove(K key) =>
            _map.Remove(key);

        public bool Remove(KeyValuePair<K, V> item) =>
            _map.Remove(item);

        public void Clear() =>
            _map.Clear();

        public bool Contains(KeyValuePair<K, V> item) =>
            _map.Contains(item);

        public bool ContainsKey(K key) =>
            _map.ContainsKey(key);

        public bool TryGetValue(K key, out V value) =>
            _map.TryGetValue(key, out value);

        public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex) =>
            _map.CopyTo(array, arrayIndex);

        public IEnumerator<KeyValuePair<K, V>> GetEnumerator() =>
            _map.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();
    }
}
