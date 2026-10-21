using JetBrains.Lifetimes;

namespace Bw.Entities.Players
{
    public interface IReadonlyPlayerCharacters
    {
        public int KeyOf(IPlayer player);
    }

    public interface IPlayerCharacters : IReadonlyPlayerCharacters
    {
        public void AddLifetimed(Lifetime lifetime, IPlayer player, int characterKey);
    }
}
