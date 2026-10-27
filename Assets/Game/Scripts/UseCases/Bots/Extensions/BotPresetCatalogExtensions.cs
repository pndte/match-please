using System;

namespace Bw.UseCases.Bots.Extensions
{
    public static class BotPresetCatalogExtensions
    {
        public static BotDifficultyConfig DifficultyOf(this BotPresetCatalog catalog, int key)
        {
            foreach (var difficulty in catalog.Difficulties)
                if (difficulty.Key == key)
                    return difficulty;

            throw new InvalidOperationException($"The bot preset catalog '{catalog.name}' has no difficulty with key {key.ToString()}.");
        }

        public static BotPersonality PersonalityOf(this BotPresetCatalog catalog, int key)
        {
            foreach (var personality in catalog.Personalities)
                if (personality.Key == key)
                    return personality;

            throw new InvalidOperationException($"The bot preset catalog '{catalog.name}' has no personality with key {key.ToString()}.");
        }

        public static BotPreset PresetOf(this BotPresetCatalog catalog, int difficultyKey, int personalityKey) =>
            new(catalog.DifficultyOf(difficultyKey), catalog.PersonalityOf(personalityKey));

        public static BotPreset DefaultPreset(this BotPresetCatalog catalog, Random random) =>
            new(catalog.DefaultDifficulty, catalog.Personalities[random.Next(catalog.Personalities.Length)]);

        public static BotDifficultyConfig DifficultyAfter(this BotPresetCatalog catalog, BotDifficultyConfig difficulty) =>
            catalog.Difficulties[(IndexOf(catalog.Difficulties, difficulty) + 1) % catalog.Difficulties.Length];

        public static BotPersonality PersonalityAfter(this BotPresetCatalog catalog, BotPersonality personality) =>
            catalog.Personalities[(IndexOf(catalog.Personalities, personality) + 1) % catalog.Personalities.Length];

        private static int IndexOf<T>(T[] items, T item) where T : class
        {
            var index = Array.IndexOf(items, item);
            if (index < 0)
                throw new InvalidOperationException($"'{item}' is not in the bot preset catalog.");

            return index;
        }
    }
}
