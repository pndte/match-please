using System;
using System.Collections.Generic;
using Bw.Entities.Extensions;
using JetBrains.Lifetimes;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bw.Entities.Pool.GameObjects
{
    public sealed class PrefabPools : IPrefabPools
    {
        private readonly Dictionary<GameObject, IPool<IPrefabInstance>> _instances = new();
        private readonly Dictionary<(GameObject Prefab, Type Facade), IPool<object>> _pools = new();
        private readonly List<LimitedPool<IPrefabInstance>> _listed = new();
        private readonly Lifetime _lifetime;
        private readonly Transform _root;
        private readonly Func<GameObject, Transform, GameObject> _instantiate;
        private readonly PoolSettings _unlisted;

        private PrefabPools(Lifetime lifetime, Transform root, Func<GameObject, Transform, GameObject> instantiate, PoolSettings unlisted)
        {
            _lifetime = lifetime;
            _root = root;
            _instantiate = instantiate;
            _unlisted = unlisted;
        }

        public static PrefabPools Create(Lifetime lifetime, PrefabPoolsConfig config, bool withViews, Func<GameObject, Transform, GameObject> instantiate)
        {
            var root = new GameObject("Pools");
            Object.DontDestroyOnLoad(root);
            var poolsLifetime = lifetime.Intersect(root.Lifetime());
            poolsLifetime.OnTermination(() => Object.Destroy(root));

            var pools = new PrefabPools(poolsLifetime, root.transform, instantiate, new PoolSettings(0, config.UnlistedMaxIdle, PoolCap.None));
            var listed = new HashSet<GameObject>();
            foreach (var pool in config.Prefabs)
            {
                if (pool.Prefab == null)
                    throw new ArgumentException("A pool in the settings has no prefab.", nameof(config));
                if (pool.InUseCap == null)
                    throw new ArgumentException($"The pool of {pool.Prefab.name} has no in-use cap.", nameof(config));
                if (!listed.Add(pool.Prefab))
                    throw new ArgumentException($"{pool.Prefab.name} is in the pool settings twice.", nameof(config));
                if (pool.ViewOnly && !withViews)
                    continue;

                var instances = pools.Instances(pool.Prefab, new PoolSettings(pool.Prewarm, pool.MaxIdle, pool.InUseCap.Cap()));
                pools._instances.Add(pool.Prefab, instances);
                pools._listed.Add(instances);
            }

            return pools;
        }

        public IPool<T> For<T>(GameObject prefab) where T : class
        {
            var key = (prefab, typeof(T));
            if (_pools.TryGetValue(key, out var pool))
                return (IPool<T>)pool;

            var created = Pool<T>(prefab);
            _pools.Add(key, created);
            return created;
        }

        public void Prewarm()
        {
            _lifetime.ThrowIfNotAlive();
            foreach (var pool in _listed)
                pool.Prewarm();
        }

        private IPool<T> Pool<T>(GameObject prefab) where T : class
        {
            if (!prefab.TryGetComponent<IResource<T>>(out _) && !prefab.TryGetComponent<T>(out _))
                throw new ArgumentException($"{prefab.name} has neither {typeof(T).Name} nor IResource<{typeof(T).Name}>.", nameof(prefab));

            if (!_instances.TryGetValue(prefab, out var instances))
            {
                instances = Instances(prefab, _unlisted);
                _instances.Add(prefab, instances);
            }

            return new PrefabPool<T>(instances);
        }

        private LimitedPool<IPrefabInstance> Instances(GameObject prefab, PoolSettings settings)
        {
            var parent = new GameObject(prefab.name).transform;
            parent.SetParent(_root, false);
            return LimitedPool<IPrefabInstance>.Create(_lifetime, lifetime => Instantiate(lifetime, prefab, parent), settings);
        }

        private IResource<IPrefabInstance> Instantiate(Lifetime lifetime, GameObject prefab, Transform parent)
        {
            var instance = _instantiate(prefab, parent);
            instance.SetActive(false);
            lifetime.OnTermination(() => Object.Destroy(instance));
            return new GameObjectResource(instance, instance.Lifetime());
        }
    }
}
