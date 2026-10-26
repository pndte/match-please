using Bw.Entities.Players;
using JetBrains.Lifetimes;

namespace Bw.Entities
{
    public interface IOwnerRegistry
    {
        public void AddOwner(Lifetime lifetime, IPlayer player);
        public IReadonlyViewableSet<IPlayer> Owners { get; }
    }
}
