namespace Bw.Entities.Players
{
    public interface IPlayer
    {
        public string Nickname { get; }
        public string CharacterKey { get; }
    }

    public sealed class Player : IPlayer
    {
        public string Nickname { get; }
        public string CharacterKey { get; }

        public Player(string nickname, string characterKey)
        {
            Nickname = nickname;
            CharacterKey = characterKey;
        }
    }
}
