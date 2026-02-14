using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using Unity.Netcode;

namespace Bw.Entities.Network
{
    public class NetworkLifetimedBehaviour : NetworkBehaviour, INetworkLifetimed
    {
        public IReadonlyProperty<Lifetime> SpawnedLifetime => _spawnedLifetime;
        private readonly ViewableProperty<Lifetime> _spawnedLifetime = new(Lifetime.Terminated);
        private LifetimeDefinition _spawnedLifetimeDefinition;

        public override void OnNetworkSpawn()
        {
            _spawnedLifetimeDefinition = new LifetimeDefinition();
            _spawnedLifetime.Value = _spawnedLifetimeDefinition.Lifetime;
        }
        
        public override void OnNetworkDespawn()
        {
            _spawnedLifetimeDefinition.Terminate();
            _spawnedLifetime.Value = Lifetime.Terminated;
        }
    }
}