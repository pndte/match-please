using Bw.UseCases.Shooting.Weapon.Pickup.Abstractions;
using UnityEngine;

namespace Bw.UseCases.Shooting.Weapon.Pickup.Extensions
{
    public static class FloorWeaponsExtensions
    {
        public static bool TryFindPickable(
            this IReadonlyFloorWeapons weapons,
            Vector2 point,
            Vector2 holder,
            WeaponPickupConfig config,
            out IFloorWeapon pickable)
        {
            var found = false;
            var closest = float.MaxValue;
            pickable = default;

            var lying = weapons.Lying;
            for (var index = 0; index < lying.Count; index++)
            {
                var weapon = lying[index];
                var distance = weapon.DistanceTo(point);
                if (distance >= closest || !config.IsPickable(holder, weapon.Position, distance, 0f))
                    continue;

                found = true;
                closest = distance;
                pickable = weapon;
            }

            return found;
        }
    }
}
