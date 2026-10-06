using System;
using System.Collections.Generic;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.Entities.Pool
{
    public sealed class LimitedPool<T> : IPool<T>
    {
        private readonly Stack<IResource<T>> _resources = new();
        private readonly Dictionary<IResource<T>, LifetimeDefinition> _resourceLifetimes = new();
        private readonly LinkedList<LifetimeDefinition> _uses = new();
        private readonly Lifetime _lifetime;
        private readonly PoolSettings _settings;
        private readonly Func<Lifetime, IResource<T>> _factory;

        private LimitedPool(Lifetime lifetime, Func<Lifetime, IResource<T>> factory, PoolSettings settings)
        {
            _settings = settings;
            _factory = factory;
            _lifetime = lifetime;
        }

        public static LimitedPool<T> Create(Lifetime lifetime, Func<Lifetime, IResource<T>> factory, PoolSettings settings)
        {
            lifetime.ThrowIfNotAlive();
            if (settings.Prewarm > settings.Limit)
                throw new ArgumentException($"A pool can't prewarm {settings.Prewarm} resources when it keeps at most {settings.Limit} free.", nameof(settings));

            var pool = new LimitedPool<T>(lifetime, factory, settings);
            pool.Prewarm();
            return pool;
        }

        public T Resource(Lifetime lifetime)
        {
            _lifetime.ThrowIfNotAlive();
            lifetime.ThrowIfNotAlive();
            _settings.Cap.Switch(this, static _ => { }, static (pool, max) => pool.ReclaimOldest(max), static (pool, max) => pool.RequireRoom(max));

            if (!_resources.TryPop(out var resource))
                resource = CreateResource();

            var use = Lifetime.DefineIntersection(lifetime, _resourceLifetimes[resource].Lifetime);
            var node = _uses.AddLast(use);
            use.Lifetime.OnTermination(() =>
            {
                _uses.Remove(node);
                if (!_resourceLifetimes.TryGetValue(resource, out var resourceDefinition))
                    return;

                if (_resources.Count >= _settings.Limit)
                {
                    resourceDefinition.Terminate();
                    return;
                }

                _resources.Push(resource);
            });

            return resource.Facade(use.Lifetime);
        }

        private void Prewarm()
        {
            while (_resources.Count < _settings.Prewarm)
                _resources.Push(CreateResource());
        }

        private IResource<T> CreateResource()
        {
            var definition = _lifetime.CreateNested();
            var resourceLifetime = definition.Lifetime;
            var resource = _factory.Invoke(resourceLifetime);
            _resourceLifetimes.AddLifetimed(resourceLifetime, new(resource, definition));
            return resource;
        }

        private void ReclaimOldest(int max)
        {
            if (_uses.Count >= max)
                _uses.First.Value.Terminate();
        }

        private void RequireRoom(int max)
        {
            if (_uses.Count >= max)
                throw new InvalidOperationException($"All {max} resources the pool may give out are in use.");
        }
    }
}
