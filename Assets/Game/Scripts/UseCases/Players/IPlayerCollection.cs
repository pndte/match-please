using Bw.Entities.Infrastructure;

namespace Bw.UseCases.Players
{
    public interface IPlayerCollection
    {
        public IViewableBiMap<int, IPlayer> ById { get; }
    }
}