using UnityEngine;

namespace Bw.Entities.Pool.GameObjects
{
    public static class PrefabPoolsExtensions
    {
        public static IPool<T> For<T>(this IPools pools, GameObject prefab) where T : class =>
            pools.For<T, GameObject>(prefab);
    }
}
