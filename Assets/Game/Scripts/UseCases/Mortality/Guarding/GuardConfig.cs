using UnityEngine;

namespace Bw.UseCases.Mortality.Guarding
{
    [CreateAssetMenu(fileName = "GuardConfig", menuName = "Configs/GuardConfig")]
    public sealed class GuardConfig : ScriptableObject
    {
        [Min(1f)] public float MaxHealth = 100f;
        [Range(0f, 1f)] public float Armor;
        [Range(0f, 360f)] public float CoverAngle = 180f;

        [Header("Stance")]
        [Range(0f, 1f)] public float SpeedShare = 0.5f;
        [Range(0f, 1f)] public float JumpHeightShare = 0.5f;

        [Header("Recovery")]
        [Min(0f)] public float RegenDelay = 2f;
        [Min(0f)] public float BrokenRegenDelay = 4f;
        [Min(0.05f)] public float RegenInterval = 0.2f;
        [Min(0.1f)] public float RegenAmount = 8f;
    }
}
