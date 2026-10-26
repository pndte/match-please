using Bw.Entities;
using Bw.Entities.Physics.Extensions;
using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Weapon.Pickup;
using Bw.UseCases.Combat.Weapon.Pickup.Extensions;
using Unity.Collections;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.UseCases.Combat.Weapon.Network.Requests
{
    public sealed class PickableWeapons
    {
        private readonly IWeaponCarriers _carriers;
        private readonly WeaponPickupConfig _config;
        private readonly WeaponPickupNetworkConfig _networkConfig;
        private readonly PhysicsBody _body;
        private readonly PhysicsQuery.QueryFilter _filter;

        public PickableWeapons(
            IWeaponCarriers carriers,
            WeaponPickupConfig config,
            WeaponPickupNetworkConfig networkConfig,
            PhysicsBody body)
        {
            _carriers = carriers;
            _config = config;
            _networkConfig = networkConfig;
            _body = body;
            _filter = new PhysicsQuery.QueryFilter(PhysicsMask.All, config.WeaponMask.AsPhysicsMask());
        }

        public bool Pickable(IWeapon weapon)
        {
            if (_carriers.ByWeapon.ContainsKey(weapon))
                return false;

            var reach = new CircleGeometry { center = _body.position, radius = _config.Reach + _networkConfig.ReachSlack };
            using var near = _body.world.OverlapGeometry(reach, _filter, Allocator.Temp);
            for (var index = 0; index < near.Length; index++)
            {
                var shape = near[index].shape;
                if (shape.TryGetComponent<IHolder<IWeapon>>(out var holder)
                    && holder.Value == weapon
                    && _config.InReach(_body.position, shape.aabb.center, _networkConfig.ReachSlack))
                    return true;
            }

            return false;
        }
    }
}
