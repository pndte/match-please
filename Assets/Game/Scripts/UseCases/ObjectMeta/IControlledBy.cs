using Bw.Entities;
using Bw.UseCases.Players;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases
{
    public interface IControlledBy : IReadonlyControlledBy
    {
        public void Set(Lifetime lifetime, IPlayer player);
        public IReadonlyViewableList<IPlayer> Users { get; }
    }
    
    public interface IReadonlyControlledBy
    {
        public IReadonlyProperty<bool> Me { get; }
    }
}