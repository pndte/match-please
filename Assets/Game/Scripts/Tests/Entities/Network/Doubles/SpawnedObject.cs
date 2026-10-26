using Bw.Entities.Network.Objects;
using Bw.Entities.Network.Variables;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.Tests.Entities.Network.Doubles
{
    public sealed class SpawnedObject : INetworkLifetimedObject
    {
        public ulong Id { get; }
        public INetInbox Inbox { get; }
        public IReadonlyProperty<Lifetime> SpawnedLifetime => _spawnedLifetime;

        private readonly ViewableProperty<Lifetime> _spawnedLifetime = new(Lifetime.Terminated);
        private LifetimeDefinition _spawn;

        public SpawnedObject(ulong id = 1, INetInbox inbox = null)
        {
            Id = id;
            Inbox = inbox;
        }

        public void Spawn()
        {
            _spawn = new LifetimeDefinition();
            _spawnedLifetime.Value = _spawn.Lifetime;
        }

        public void Despawn()
        {
            _spawn.Terminate();
            _spawnedLifetime.Value = Lifetime.Terminated;
        }
    }
}
