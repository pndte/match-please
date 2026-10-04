using Bw.Entities.Infrastructure;
using Bw.Entities.Network;

namespace Bw.UseCases.Players
{
    public interface IClientPlayerCollection
    {
        public IViewableBiMap<IClient, IPlayer> ByClient { get; }
    }
}