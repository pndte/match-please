using Bw.UseCases.Character;
using UnityEngine;

namespace Bw.UseCases.Shooting.Melee
{
    [CreateAssetMenu(fileName = "MeleeWeaponConfig", menuName = "Configs/MeleeWeaponConfig")]
    public sealed class MeleeWeaponConfig : ScriptableObject, IHitConfig
    {
        [Header("Swing")]
        [Min(0.01f)] public float SwingTime = 0.28f;
        [Min(0f)] public float StrikeTime = 0.1f;
        [Min(1)] public float TurnSpeed = 2160f;

        [Header("Hit")]
        public float Damage = 12f;
        [Min(0)] public float Knockback = 9f;
        [Min(0.01f)] public float Reach = 1.4f;
        [Min(0f)] public float Radius = 0.35f;
        public LayerMask HitMask;

        float IHitConfig.Damage => Damage;
        float IHitConfig.Knockback => Knockback;
    }
}
