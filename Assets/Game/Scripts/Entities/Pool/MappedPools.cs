using System;
using System.Collections.Generic;

namespace Bw.Entities.Pool
{
    public sealed class MappedPools : IPools
    {
        private readonly Dictionary<Type, IPool<object>> _pools;

        public MappedPools(Dictionary<Type, IPool<object>> pools)
        {
            _pools = pools;
        }

        public IPool<T> For<T>()
        {
            if (!_pools.TryGetValue(typeof(T), out var pool))
                throw new KeyNotFoundException($"This container does not maintain a pool with Type: {typeof(T)}");

            return (IPool<T>)pool;
        }
    }
}
