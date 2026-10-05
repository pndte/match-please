using Bw.Entities.Players;
using JetBrains.Lifetimes;

namespace Bw.Entities
{
    public interface IControlledBy : IReadonlyControlledBy //TODO: переименовать по принципу с Ownership и убрать наследование от IControlledBy
                                                           //который должен стать IControlledBy.
    {
        public void Set(Lifetime lifetime, IPlayer player);
        public IReadonlyViewableList<IPlayer> Users { get; }
    }
}