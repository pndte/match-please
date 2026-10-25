using Bw.Entities.Network.Objects;
using Bw.Entities.Network.Variables;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.Tests.UseCases.Mortality.Doubles
{
    public sealed class SpawnedObject : INetworkLifetimedObject
    {
        public ulong Id => 0;
        public INetEntries NetEntries { get; }
        public IReadonlyProperty<Lifetime> SpawnedLifetime => _spawned;

        private readonly ViewableProperty<Lifetime> _spawned = new(Lifetime.Terminated);

        public SpawnedObject(INetEntries entries)
        {
            NetEntries = entries;
        }

        public void Spawn(Lifetime lifetime) =>
            _spawned.Value = lifetime;
    }
}
