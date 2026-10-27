using System;

namespace Bw.UseCases.Bots
{
    [Serializable]
    public struct StartingBot
    {
        public string Nickname;
        public BotDifficultyConfig Difficulty;
        public BotPersonality Personality;
    }
}
