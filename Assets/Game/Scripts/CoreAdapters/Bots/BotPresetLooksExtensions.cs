using System;
using Bw.UseCases.Bots;

namespace Bw.CoreAdapters.Bots
{
    public static class BotPresetLooksExtensions
    {
        public static string NameOf(this BotPresetLooksConfig looks, BotDifficultyConfig difficulty)
        {
            foreach (var look in looks.Difficulties)
                if (look.Difficulty == difficulty)
                    return look.DisplayName;

            throw new InvalidOperationException($"Difficulty '{difficulty.name}' has no name in '{looks.name}'.");
        }

        public static string NameOf(this BotPresetLooksConfig looks, BotPersonality personality)
        {
            foreach (var look in looks.Personalities)
                if (look.Personality == personality)
                    return look.DisplayName;

            throw new InvalidOperationException($"Personality '{personality.name}' has no name in '{looks.name}'.");
        }
    }
}
