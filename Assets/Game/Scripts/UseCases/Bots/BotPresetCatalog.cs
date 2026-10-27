using System;
using UnityEngine;

namespace Bw.UseCases.Bots
{
    [CreateAssetMenu(fileName = "BotPresetCatalog", menuName = "Configs/BotPresetCatalog")]
    public sealed class BotPresetCatalog : ScriptableObject
    {
        public BotDifficultyConfig[] Difficulties = Array.Empty<BotDifficultyConfig>();
        public BotPersonality[] Personalities = Array.Empty<BotPersonality>();
        public BotDifficultyConfig DefaultDifficulty;
    }
}
