using System;
using Bw.Entities.Network.Objects;
using Bw.Entities.Network.Variables;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.Tests.UseCases.PropsMatchBots.Doubles
{
    public sealed class StubNetworkObject : INetworkLifetimedObject
    {
        public ulong Id => 1;
        public INetInbox Inbox => throw new NotSupportedException("The stub network object has no net inbox.");
        public IReadonlyProperty<Lifetime> SpawnedLifetime => _spawnedLifetime;

        private readonly ViewableProperty<Lifetime> _spawnedLifetime = new(Lifetime.Terminated);
        private readonly SequentialLifetimes _spawns;

        public StubNetworkObject(Lifetime lifetime)
        {
            _spawns = new SequentialLifetimes(lifetime);
        }

        public void Spawn() =>
            _spawnedLifetime.Value = _spawns.Next();

        public void Despawn()
        {
            _spawns.TerminateCurrent();
            _spawnedLifetime.Value = Lifetime.Terminated;
        }
    }
}
