using Bw.Entities.Pool;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.Entities.Network.Objects
{
    public static class PooledNetworkObjectExtensions
    {
        public static void Spawn(this IPool<PooledNetworkObject> pool, Lifetime lifetime, Vector3 position, Quaternion rotation) =>
            pool.Resource(lifetime).Spawn(position, rotation);
    }
}
