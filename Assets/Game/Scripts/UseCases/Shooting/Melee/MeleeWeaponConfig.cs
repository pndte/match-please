using UnityEngine;

namespace Bw.UseCases.Shooting.Melee
{
    [CreateAssetMenu(fileName = "MeleeWeaponConfig", menuName = "Configs/MeleeWeaponConfig")]
    public sealed class MeleeWeaponConfig : ScriptableObject
    {
        [Header("Swing")]
        [Min(0.01f)] public float SwingTime = 0.28f;
        [Min(0f)] public float StrikeTime = 0.1f;
        [Min(1)] public float TurnSpeed = 2160f;

        [Header("Hit")]
        public float Damage = 24f;
        [Min(0)] public float Knockback = 9f;
        [Min(0.01f)] public float Reach = 1.4f;
        [Min(0f)] public float Radius = 0.35f;
        public LayerMask HitMask;

        [Header("Strong punch")]
        [Min(0f)] public float MinChargeTime = 0.3f;
        [Min(0.01f)] public float FullChargeTime = 1f;
        [Min(0f)] public float FullChargeHoldTime = 0.8f;
        [Min(0.01f)] public float StrongSwingTime = 0.45f;
        [Min(0f)] public float StrongStrikeTime = 0.1f;
        public float StrongBaseDamage = 24f;
        public float StrongDamage = 80f;
        [Min(0)] public float StrongBaseKnockback = 9f;
        [Min(0)] public float StrongKnockback = 24f;
        [Min(0f)] public float RecoveryTime = 0.2f;
    }
}
