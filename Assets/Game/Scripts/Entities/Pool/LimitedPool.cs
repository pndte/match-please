using System;
using System.Collections.Generic;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.Entities.Pool
{
    public sealed class LimitedPool<T> : IPool<T>, IPrewarmable
    {
        private readonly Stack<IResource<T>> _resources = new();
        private readonly Dictionary<IResource<T>, LifetimeDefinition> _resourceLifetimes = new();
        private readonly LinkedList<LifetimeDefinition> _uses = new();
        private readonly Lifetime _lifetime;
        private readonly PoolSettings _settings;
        private readonly Func<Lifetime, IResource<T>> _factory;

        public LimitedPool(Lifetime lifetime, Func<Lifetime, IResource<T>> factory, PoolSettings settings)
        {
            _settings = settings;
            _factory = factory;
            _lifetime = lifetime;
        }

        public void Prewarm()
        {
            _lifetime.ThrowIfNotAlive();
            if (_settings.Prewarm > _settings.Limit)
                throw new InvalidOperationException(
                    $"A pool can't prewarm {_settings.Prewarm} resources when it keeps at most {_settings.Limit} free.");

            while (_resources.Count < _settings.Prewarm)
                _resources.Push(Create());
        }

        public T Resource(Lifetime lifetime)
        {
            _lifetime.ThrowIfNotAlive();
            lifetime.ThrowIfNotAlive();
            _settings.Cap.Switch(this, static _ => { }, static (pool, max) => pool.ReclaimOldest(max), static (pool, max) => pool.RequireRoom(max));

            if (!_resources.TryPop(out var resource))
                resource = Create();

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

        private IResource<T> Create()
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
