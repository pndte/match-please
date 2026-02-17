using Bw.Entities.Network.Variables;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using Unity.Netcode;
using Zenject;

namespace Bw.Entities.Network.Objects
{
    public class NetworkLifetimedBehaviour : NetworkBehaviour, INetworkLifetimedObject
    {
        public ulong Id => NetworkObject.NetworkObjectId;
        public INetVariablesTable NetVariablesTable { get; private set; }
        public IReadonlyProperty<Lifetime> SpawnedLifetime => _spawnedLifetime;
        private readonly ViewableProperty<Lifetime> _spawnedLifetime = new(Lifetime.Terminated);
        private LifetimeDefinition _spawnedLifetimeDefinition;

        [Inject]
        private void Construct(INetVariablesTable netVariablesTable)
        {
            NetVariablesTable = netVariablesTable;
        }

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