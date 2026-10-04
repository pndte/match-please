using Bw.Entities.Network.Variables;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network.Objects
{
    public interface INetworkLifetimedObject
    {
        public ulong Id { get; }
        public INetEntries NetEntries { get; }
        public IReadonlyProperty<Lifetime> SpawnedLifetime { get; }
    }
}