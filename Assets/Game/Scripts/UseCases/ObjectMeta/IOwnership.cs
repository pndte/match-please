using Bw.Entities;
using Bw.UseCases.Players;
using JetBrains.Lifetimes;

namespace Bw.UseCases
{
    public interface IOwnership : IReadonlyOwnership //TODO: Должны быть два разных интерфейса. IReadonlyOwnership надо назвать просто IOwnership
    {
        public void AddOwner(Lifetime lifetime, IPlayer player);
        public IReadonlyViewableList<IPlayer> Owners { get; }
    }
}