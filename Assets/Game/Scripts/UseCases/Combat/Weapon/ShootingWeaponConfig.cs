using Bw.UseCases.Mortality;
using UnityEngine;

namespace Bw.UseCases.Combat.Weapon
{
    [CreateAssetMenu(fileName = "ShootingWeaponConfig", menuName = "Configs/ShootingWeaponConfig")]
    public class ShootingWeaponConfig : ScriptableObject, IHitConfig
    {
        public AmmoConfig AmmoSettings;
        [Min(0)] public float ShootCooldown;
        [Min(0)] public float ReloadTime;
        public float Damage;
        [Min(0)] public float Knockback;
        [Min(1)] public float TurnSpeed = 600f;
        [Min(0)] public float UnheldDespawnTime = 25f;

        [Header("Spread (degrees every shot throws the barrel off the cursor; the barrel comes back at Return degrees a second)")]
        public SpreadConfig Spread = new();

        [Header("Pellets (a shot fans Count pellets evenly across Cone degrees; Damage and Knockback count per pellet)")]
        public PelletsConfig Pellets = new();

        float IHitConfig.Damage => Damage;
        float IHitConfig.Knockback => Knockback;
    }
}
