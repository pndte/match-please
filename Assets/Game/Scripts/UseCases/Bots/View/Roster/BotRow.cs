namespace Bw.UseCases.Bots.View.Roster
{
    public readonly struct BotRow
    {
        public readonly string Nickname;
        public readonly string Difficulty;
        public readonly string Personality;

        public BotRow(string nickname, string difficulty, string personality)
        {
            Nickname = nickname;
            Difficulty = difficulty;
            Personality = personality;
        }
    }
}
