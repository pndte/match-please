using Bw.UseCases.Players;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.Entities
{
    public interface IOwnership : IReadonlyOwnership
    {
        public void AddOwner(Lifetime lifetime, IPlayer player);
        public IReadonlyViewableList<IPlayer> Owners { get; }
    }
    
    public interface IReadonlyOwnership
    {
        public IReadonlyProperty<bool> Mine { get; }
    }
}