using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network
{
    public interface INetworkLifetimed : ILifetimed
    {
        public IReadonlyProperty<Lifetime> SpawnedLifetime { get; }
        public IReadonlyProperty<Lifetime> AliveLifetime { get; }
        public IReadonlyProperty<Lifetime> OwnerLifetime { get; }
    }
}