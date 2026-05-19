using Bw.Entities.Network.Variables;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network.Objects
{
    public interface INetworkLifetimedObject
    {
        public ulong Id { get; }
        public INetVariablesTable NetVariablesTable { get; }
        public IReadonlyProperty<Lifetime> SpawnedLifetime { get; }
    }
}