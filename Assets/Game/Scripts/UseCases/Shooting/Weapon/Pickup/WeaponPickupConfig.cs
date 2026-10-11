using System;
using UnityEngine;

namespace Bw.UseCases.Shooting.Weapon.Pickup
{
    [Serializable]
    public sealed class WeaponPickupConfig
    {
        [Min(0.1f)] public float Reach = 2.5f;
        [Min(0f)] public float HoverRadius = 0.35f;
        public LayerMask WeaponMask;
    }
}
