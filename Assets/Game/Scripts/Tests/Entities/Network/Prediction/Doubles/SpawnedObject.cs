using Bw.Entities.Network.Objects;
using Bw.Entities.Network.Variables;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.Tests.Entities.Network.Prediction.Doubles
{
    public sealed class SpawnedObject : INetworkLifetimedObject
    {
        public ulong Id { get; }
        public INetInbox Inbox => null;
        public IReadonlyProperty<Lifetime> SpawnedLifetime => _spawned;

        private readonly ViewableProperty<Lifetime> _spawned = new(Lifetime.Terminated);

        private LifetimeDefinition _definition;

        public SpawnedObject(ulong id = 0UL)
        {
            Id = id;
        }

        public void Spawn()
        {
            _definition = new LifetimeDefinition();
            _spawned.Value = _definition.Lifetime;
        }

        public void Despawn()
        {
            _definition.Terminate();
            _spawned.Value = Lifetime.Terminated;
        }
    }
}
