using System;
using UnityEngine;

namespace Bw.UseCases.Shooting.Weapon.Network
{
    [Serializable]
    public sealed class WeaponPickupNetworkConfig
    {
        [Min(0f)] public float ReachSlack = 1f;
    }
}
