using Bw.Entities;
using Bw.UseCases.Players;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases
{
    public interface IControlledBy : IReadonlyControlledBy //TODO: переименовать по принципу с Ownership и убрать наследование от IControlledBy
                                                           //который должен стать IControlledBy.
    {
        public void Set(Lifetime lifetime, IPlayer player);
        public IReadonlyViewableList<IPlayer> Users { get; }
    }
}