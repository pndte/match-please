namespace Bw.UseCases.Bots.Network.Room
{
    public readonly struct BotSeat
    {
        public readonly int PlayerId;
        public readonly string Nickname;
        public readonly BotPreset Preset;

        public BotSeat(int playerId, string nickname, BotPreset preset)
        {
            PlayerId = playerId;
            Nickname = nickname;
            Preset = preset;
        }
    }
}
