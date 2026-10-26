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
            float reachSlack) =>
            pointDistance <= config.HoverRadius && config.InReach(holder, weapon, reachSlack);

        public static bool InReach(
            this WeaponPickupConfig config,
            Vector2 holder,
            Vector2 weapon,
            float reachSlack)
        {
            var reach = config.Reach + reachSlack;
            return (weapon - holder).sqrMagnitude <= reach * reach;
        }
    }
}
