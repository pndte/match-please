using UnityEngine;

namespace Bw.UseCases.Character.View.HealthBar
{
    [CreateAssetMenu(fileName = "HealthBarConfig", menuName = "Configs/HealthBarConfig")]
    public sealed class HealthBarConfig : ScriptableObject
    {
        [Header("Place")]
        public Vector2 HeadOffset = new(0f, 1.4f);
        [Min(0.1f)] public float SnapDistance = 4f;

        [Header("Inertia")]
        [Min(0f)] public float Stiffness = 676f;
        [Min(0f)] public float Damping = 18.2f;
        [Min(0.01f)] public float MaxLag = 0.45f;
        public float TiltPerAcceleration = 0.05f;
        [Min(0f)] public float MaxTilt = 12f;

        [Header("Colors")]
        public Color OwnFill = new(0.47f, 0.84f, 0.33f);
        public Color OwnShade = new(0.18f, 0.55f, 0.2f);
        public Color OtherFill = new(1f, 0.36f, 0.27f);
        public Color OtherShade = new(0.7f, 0.1f, 0.12f);

        [Header("Notches")]
        [Min(1f)] public float NotchHealth = 25f;

        [Header("Hurt")]
        [Min(0f)] public float TrailDelay = 0.35f;
        [Min(0.01f)] public float TrailDrain = 0.4f;
        [Min(0.01f)] public float FlashTime = 0.18f;
        public float ShakeStrength = 3f;
        [Min(0.01f)] public float ShakeTime = 0.25f;
        [Min(1)] public int ShakeVibrato = 18;
        public float HurtPunch = 0.14f;

        [Header("Heal")]
        [Min(0.01f)] public float HealTime = 0.45f;
        [Min(0.01f)] public float HealGlowTime = 0.6f;

        [Header("Low")]
        [Range(0f, 1f)] public float LowShare = 0.3f;
        [Min(0.01f)] public float LowFadeTime = 0.3f;

        [Header("Show")]
        [Min(0.01f)] public float AppearTime = 0.3f;
        public float AppearOvershoot = 2.2f;
        [Min(0f)] public float DeathFadeDelay = 0.45f;
        [Min(0.01f)] public float DeathFadeTime = 0.3f;
    }
}
