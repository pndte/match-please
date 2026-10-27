namespace Bw.UseCases.Bots.Network.Requests
{
    public readonly struct BotPresetChange
    {
        public readonly int PlayerId;
        public readonly int DifficultyKey;
        public readonly int PersonalityKey;

        public BotPresetChange(int playerId, int difficultyKey, int personalityKey)
        {
            PlayerId = playerId;
            DifficultyKey = difficultyKey;
            PersonalityKey = personalityKey;
        }
    }
}
