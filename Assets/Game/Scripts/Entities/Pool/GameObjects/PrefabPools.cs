using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.Entities.Pool.GameObjects
{
    public static class PrefabPools
    {
        public static LimitedPool<T> Create<T, TPrefab>(Lifetime lifetime, TPrefab prefab, PoolSettings settings)
            where TPrefab : Component, IResource<T>
        {
            var root = new GameObject(prefab.name);
            lifetime.OnTermination(() => Object.Destroy(root));
            return new LimitedPool<T>(lifetime, resourceLifetime => Instantiate<T, TPrefab>(resourceLifetime, prefab, root.transform), settings);
        }

        private static IResource<T> Instantiate<T, TPrefab>(Lifetime lifetime, TPrefab prefab, Transform parent)
            where TPrefab : Component, IResource<T>
        {
            var instance = Object.Instantiate(prefab, parent);
            instance.gameObject.SetActive(false);
            lifetime.OnTermination(() => Object.Destroy(instance.gameObject));
            return new GameObjectResource<T>(instance.gameObject, instance);
        }
    }
}
