using Bw.Entities.Infrastructure;
using Bw.Entities.Players;

namespace Bw.Entities.Network
{
    public interface IClientPlayerCollection
    {
        public IViewableBiMap<IClient, IPlayer> ByClient { get; }
    }
}