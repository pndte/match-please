using UnityEngine;

namespace Bw.UseCases.Shooting.Weapon
{
    [CreateAssetMenu(fileName = "ShootingWeaponConfig", menuName = "Configs/ShootingWeaponConfig")]
    public class ShootingWeaponConfig : ScriptableObject
    {
        public AmmoConfig AmmoSettings;
        [Min(0)] public float ShootCooldown;
        [Min(0)] public float ReloadTime;
        public float Damage;
    }
}