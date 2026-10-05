using Bw.Entities.Players;
using JetBrains.Lifetimes;

namespace Bw.Entities
{
    public interface IOwnershipController
    {
        public void AddOwner(Lifetime lifetime, IPlayer player);
        public IReadonlyViewableList<IPlayer> Owners { get; }
    }
}