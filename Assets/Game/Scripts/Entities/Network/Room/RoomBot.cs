namespace Bw.Entities.Network
{
    public readonly struct RoomBot
    {
        public readonly string Nickname;
        public readonly int DifficultyKey;
        public readonly int PersonalityKey;

        public RoomBot(string nickname, int difficultyKey, int personalityKey)
        {
            Nickname = nickname;
            DifficultyKey = difficultyKey;
            PersonalityKey = personalityKey;
        }
    }
}
