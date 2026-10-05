using Bw.Entities.Network.Variables;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network.Objects
{
    public interface INetworkLifetimedObject
    {
        public ulong Id { get; } //TODO: никто не использует, и объект не должен знать свой id — удалить
        public INetEntries NetEntries { get; }
        public IReadonlyProperty<Lifetime> SpawnedLifetime { get; }
    }
}