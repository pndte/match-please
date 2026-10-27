using UnityEngine;

namespace Bw.UseCases.Bots
{
    [CreateAssetMenu(fileName = "BotDifficultyConfig", menuName = "Configs/BotDifficultyConfig")]
    public sealed class BotDifficultyConfig : ScriptableObject
    {
        public BotBrainKind Brain = BotBrainKind.Goap;
        public bool Moving = true;
        public bool Hostile = true;

        [Header("Sight")]
        [Min(0f)] public float ReactionTime = 0.5f;
        [Min(0f)] public float MemoryTime = 3f;

        [Header("Aim")]
        [Min(0f)] public float AimError = 6f;
        [Min(0f)] public float AimWander = 0.7f;

        [Header("Fire")]
        [Min(0.05f)] public float BurstTime = 0.6f;
        [Min(0f)] public float PauseTime = 0.5f;

        [Header("Melee")]
        public bool Brawling;
        public bool ChargingPunches;
        [Min(0.3f)] public float ChargeTime = 0.9f;

        [Header("Guard")]
        public bool Parrying;

        [Header("Props")]
        public bool Shielding;
    }
}
