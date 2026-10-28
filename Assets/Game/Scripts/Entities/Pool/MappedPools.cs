using System;
using System.Collections.Generic;
using System.Linq;

namespace Bw.Entities.Pool
{
    public sealed class MappedPools : IPools
    {
        private readonly Dictionary<Type, IPool<object>> _pools;
        private readonly Dictionary<Type, object> _keyed;
        private readonly Dictionary<Type, object> _resolved = new();

        public MappedPools(Dictionary<Type, IPool<object>> pools, Dictionary<Type, object> keyed)
        {
            _pools = pools;
            _keyed = keyed;
        }

        public IPool<T> For<T>() where T : class
        {
            if (!_pools.TryGetValue(typeof(T), out var pool))
                throw new KeyNotFoundException($"This container does not maintain a pool with Type: {typeof(T)}");

            return (IPool<T>)pool;
        }

        public IPool<T> For<T, TKey>(TKey key) where T : class =>
            ((IKeyedPools<TKey>)Keyed(typeof(TKey))).For<T>(key);

        private object Keyed(Type key)
        {
            if (_resolved.TryGetValue(key, out var pools))
                return pools;

            pools = Nearest(key);
            _resolved.Add(key, pools);
            return pools;
        }

        private object Nearest(Type key)
        {
            for (var type = key; type != null; type = type.BaseType)
                if (_keyed.TryGetValue(type, out var pools))
                    return pools;

            var matches = key.GetInterfaces().Where(_keyed.ContainsKey).ToArray();
            return matches.Length switch
            {
                1 => _keyed[matches[0]],
                0 => throw new KeyNotFoundException($"This container does not maintain pools keyed by {key}"),
                _ => throw new InvalidOperationException($"{key} matches pools keyed by {string.Join(", ", matches.Select(match => match.Name))}")
            };
        }
    }
}
