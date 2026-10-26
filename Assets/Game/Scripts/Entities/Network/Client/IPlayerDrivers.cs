using Bw.Entities.Players;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network
{
    public interface IReadonlyPlayerDrivers
    {
        public IViewableMap<IClient, IPlayer> ByClient { get; }
        public IReadonlyProperty<PlayerDriver> Of(IPlayer player);
        public IPlayer PlayerOf(IClient client);
    }

    public interface IPlayerDrivers : IReadonlyPlayerDrivers
    {
        public void AddClientDriven(Lifetime lifetime, IPlayer player, IClient client);
        public void AddServerDriven(Lifetime lifetime, IPlayer player);
        public void ChangeDriver(IPlayer player, PlayerDriver driver); //TODO: lf сюда
    }
}
