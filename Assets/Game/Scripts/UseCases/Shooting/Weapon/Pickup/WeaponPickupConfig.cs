using UnityEngine;

namespace Bw.UseCases.Shooting.Weapon.Pickup
{
    [CreateAssetMenu(fileName = "WeaponPickupConfig", menuName = "Configs/WeaponPickupConfig")]
    public sealed class WeaponPickupConfig : ScriptableObject
    {
        [Min(0.1f)] public float Reach = 2.5f;
        [Min(0f)] public float HoverRadius = 0.35f;
        public LayerMask WeaponMask;
    }
}
