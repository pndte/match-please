using Bw.UseCases.Character;
using UnityEngine;

namespace Bw.UseCases.Shooting.Weapon
{
    [CreateAssetMenu(fileName = "ShootingWeaponConfig", menuName = "Configs/ShootingWeaponConfig")]
    public class ShootingWeaponConfig : ScriptableObject, IHitConfig
    {
        public AmmoConfig AmmoSettings;
        [Min(0)] public float ShootCooldown;
        [Min(0)] public float ReloadTime;
        public float Damage;
        [Min(0)] public float Knockback;
        [Min(0)] public float UnheldDespawnTime = 25f;

        [Header("Spread (degrees every shot throws the cursor around the shooter; the player brings it back)")]
        public SpreadConfig Spread = new();

        float IHitConfig.Damage => Damage;
        float IHitConfig.Knockback => Knockback;
    }
}
