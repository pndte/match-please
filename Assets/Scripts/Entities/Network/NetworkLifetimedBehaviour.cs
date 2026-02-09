using System;
using System.Collections.Generic;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using Unity.Netcode;

namespace Bw.Entities.Network
{
    public class NetworkLifetimedBehaviour : NetworkBehaviour, INetworkLifetimed
    {
        public IReadonlyProperty<Lifetime> SpawnedLifetime => _spawnedLifetime;
        public IReadonlyProperty<Lifetime> AliveLifetime => _aliveLifetime;
        public IReadonlyProperty<Lifetime> OwnerLifetime => _ownerLifetime;

        private readonly ViewableProperty<Lifetime> _spawnedLifetime = new(Lifetime.Terminated);
        private readonly ViewableProperty<Lifetime> _aliveLifetime = new(Lifetime.Terminated);
        private readonly ViewableProperty<Lifetime> _ownerLifetime = new(Lifetime.Terminated);
        private readonly LifetimeDefinition _aliveLifetimeDefinition = new();
        private readonly List<Action<Lifetime>> _handlers = new();
        private LifetimeDefinition _spawnedLifetimeDefinition;
        private SequentialLifetimes _seqOwnerLifetimes;

        private void Awake()
        {
            _aliveLifetime.Value = _aliveLifetimeDefinition.Lifetime;
        }

        public override void OnNetworkSpawn()
        {
            _spawnedLifetimeDefinition = new LifetimeDefinition();
            _spawnedLifetime.Value = _spawnedLifetimeDefinition.Lifetime;
            _seqOwnerLifetimes = new SequentialLifetimes(_spawnedLifetime.Value);
            
            foreach (var handler in _handlers)
            {
                handler.Invoke(_spawnedLifetime.Value);
            }
        }
        public override void OnNetworkDespawn()
        {
            _spawnedLifetimeDefinition.Terminate();
            _spawnedLifetime.Value = Lifetime.Terminated;
            
            _handlers.Clear(); // TODO: mb не стоит?
        }

        public override void OnGainedOwnership()
        {
            _ownerLifetime.Value = _seqOwnerLifetimes.Next();
        }

        public override void OnLostOwnership()
        {
            _seqOwnerLifetimes.TerminateCurrent();
        }

        public void WhenAlive(Action<Lifetime> handler)
        {
            if (_spawnedLifetime.Value != Lifetime.Terminated)
                handler(_spawnedLifetime.Value);
            else
                _handlers.Add(handler);
        }

        public override void OnDestroy()
        {
            _aliveLifetimeDefinition.Terminate();
        }
    }
}