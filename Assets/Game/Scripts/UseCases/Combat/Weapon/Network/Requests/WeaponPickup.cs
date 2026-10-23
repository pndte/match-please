using UnityEngine;

namespace Bw.UseCases.Combat.Weapon.Network.Requests
{
    public readonly struct WeaponPickup
    {
        public readonly Vector2 Point;

        public WeaponPickup(Vector2 point)
        {
            Point = point;
        }
    }
}
