namespace Bw.Entities.Players
{
    public interface IPlayer
    {
        public string Nickname { get; }
    }

    public sealed class Player : IPlayer
    {
        public string Nickname { get; }

        public Player(string nickname)
        {
            Nickname = nickname;
        }
    }
}
