using System;
using System.Collections.Generic;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using Unity.Netcode;

namespace Bw.Entities.Network
{
    public class NetworkLifetimedBehaviour : NetworkBehaviour, ILifetimed
    {
        public IReadonlyProperty<Lifetime> SpawnedLifetime => _spawnedLifetime;
        
        private readonly ViewableProperty<Lifetime> _spawnedLifetime = new(Lifetime.Terminated);
        private LifetimeDefinition _spawnedLifetimeDefinition;
        private List<Action<Lifetime>> _handlers = new();
        
        public override void OnNetworkSpawn()
        {
            _spawnedLifetimeDefinition = new LifetimeDefinition();
            _spawnedLifetime.Value = _spawnedLifetimeDefinition.Lifetime;
            
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

        public void WhenAlive(Action<Lifetime> handler)
        {
            if (_spawnedLifetime.Value != Lifetime.Terminated)
                handler(_spawnedLifetime.Value);
            else
                _handlers.Add(handler);
        }
    }
}