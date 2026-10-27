using System;
using Bw.UseCases.Bots;
using UnityEngine;

namespace Bw.CoreAdapters.Bots
{
    [CreateAssetMenu(fileName = "BotPresetLooksConfig", menuName = "Configs/BotPresetLooksConfig")]
    public sealed class BotPresetLooksConfig : ScriptableObject
    {
        public DifficultyLook[] Difficulties = Array.Empty<DifficultyLook>();
        public PersonalityLook[] Personalities = Array.Empty<PersonalityLook>();
    }

    [Serializable]
    public struct DifficultyLook
    {
        public BotDifficultyConfig Difficulty;
        public string DisplayName;
    }

    [Serializable]
    public struct PersonalityLook
    {
        public BotPersonality Personality;
        public string DisplayName;
    }
}
