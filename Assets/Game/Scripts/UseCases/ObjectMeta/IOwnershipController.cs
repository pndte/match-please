using Bw.Entities;
using Bw.UseCases.Players;
using JetBrains.Lifetimes;

namespace Bw.UseCases
{
    public interface IOwnershipController
    {
        public void AddOwner(Lifetime lifetime, IPlayer player);
        public IReadonlyViewableList<IPlayer> Owners { get; }
    }
}