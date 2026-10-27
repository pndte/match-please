namespace Bw.UseCases.Bots
{
    public readonly struct BotPreset
    {
        public readonly BotDifficultyConfig Difficulty;
        public readonly BotPersonality Personality;

        public BotPreset(BotDifficultyConfig difficulty, BotPersonality personality)
        {
            Difficulty = difficulty;
            Personality = personality;
        }
    }
}
