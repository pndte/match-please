using System;
using Bw.UseCases.Character;

namespace Bw.UseCases.Bots
{
    [Serializable]
    public struct BotEntry
    {
        public string Nickname;
        public CharacterDescription Character;
        public BotDifficultyConfig Difficulty;
        public BotPersonality Personality;
    }
}
