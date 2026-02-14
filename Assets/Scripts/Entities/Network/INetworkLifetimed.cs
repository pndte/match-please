using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network
{
    public interface INetworkLifetimed
    {
        public IReadonlyProperty<Lifetime> SpawnedLifetime { get; }
    }
}