using System;
using Bw.Entities.Network.Objects;
using Bw.Entities.Network.Variables;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.Tests.UseCases.Combat.Weapon.Doubles
{
    public sealed class FakeNetworkObject : INetworkLifetimedObject
    {
        public ulong Id => throw new NotSupportedException("The fake network object has no id.");
        public INetInbox Inbox => throw new NotSupportedException("The fake network object has no net inbox.");
        public IReadonlyProperty<Lifetime> SpawnedLifetime => _spawnedLifetime;

        private readonly ViewableProperty<Lifetime> _spawnedLifetime = new(Lifetime.Terminated);

        private LifetimeDefinition _spawned;

        public void Spawn()
        {
            if (_spawned != null)
                throw new InvalidOperationException("The fake network object is spawned already.");

            _spawned = new LifetimeDefinition();
            _spawnedLifetime.Value = _spawned.Lifetime;
        }

        public void Despawn()
        {
            if (_spawned == null)
                throw new InvalidOperationException("The fake network object is not spawned.");

            _spawned.Terminate();
            _spawned = null;
            _spawnedLifetime.Value = Lifetime.Terminated;
        }
    }
}
