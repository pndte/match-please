using Bw.Entities.Players;
using JetBrains.Lifetimes;

namespace Bw.Entities
{
    public interface IControllerRegistry
    {
        public void Set(Lifetime lifetime, IPlayer player);
        public IReadonlyViewableList<IPlayer> Users { get; }
    }
}
