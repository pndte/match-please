using Bw.Entities.Players;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network
{
    public interface IReadonlyPlayerIds
    {
        public int IdOf(IPlayer player);
    }

    public interface IPlayerIds : IReadonlyPlayerIds
    {
        public void AddLifetimed(Lifetime lifetime, IPlayer player);
    }
}
