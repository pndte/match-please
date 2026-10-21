using Bw.Entities.Players;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network
{
    public interface IPlayerEntrance
    {
        public void Admit(
            Lifetime lifetime,
            IPlayer player,
            int characterKey,
            PlayerDriver driver);
    }
}
