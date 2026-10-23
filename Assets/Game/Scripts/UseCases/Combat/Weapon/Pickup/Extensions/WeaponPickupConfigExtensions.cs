using UnityEngine;

namespace Bw.UseCases.Combat.Weapon.Pickup.Extensions
{
    public static class WeaponPickupConfigExtensions
    {
        public static bool IsPickable(
            this WeaponPickupConfig config,
            Vector2 holder,
            Vector2 weapon,
            float pointDistance,
            float reachSlack)
        {
            var reach = config.Reach + reachSlack;
            return pointDistance <= config.HoverRadius && (weapon - holder).sqrMagnitude <= reach * reach;
        }
    }
}
