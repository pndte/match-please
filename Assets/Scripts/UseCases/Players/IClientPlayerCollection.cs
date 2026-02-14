using Bw.Entities;
using Bw.Entities.Infrastructure;

namespace Bw.UseCases.Players
{
    public interface IClientPlayerCollection
    {
        public IViewableBiMap<IClient, IPlayer> ByClient { get; }
    }
}