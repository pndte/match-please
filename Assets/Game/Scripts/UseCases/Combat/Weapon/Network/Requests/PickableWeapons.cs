using Bw.Entities;
using Bw.Entities.Physics.Extensions;
using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Weapon.Pickup;
using Bw.UseCases.Combat.Weapon.Pickup.Extensions;
using Unity.Collections;
using UnityEngine;
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

        public bool TryFindNear(Vector2 point, out IWeapon pickable)
        {
            var found = false;
            var closest = float.MaxValue;
            pickable = default;

            var hover = new CircleGeometry { center = point, radius = _config.HoverRadius };
            using var near = _body.world.OverlapGeometry(hover, _filter, Allocator.Temp); //TODO: точку клиент берёт со своей картинки, а летящее оружие он видит с отставанием интерполяции — подбор на лету может промахнуться; надёжнее, если запрос назовёт само оружие (id ↔ оружие на сетевой границе)
            for (var index = 0; index < near.Length; index++)
            {
                var shape = near[index].shape;
                if (!shape.TryGetComponent<IHolder<IWeapon>>(out var weapon) || _carriers.ByWeapon.ContainsKey(weapon.Value))
                    continue;

                var distance = shape.DistanceTo(point);
                if (distance >= closest || !_config.IsPickable(_body.position, shape.aabb.center, distance, _networkConfig.ReachSlack))
                    continue;

                found = true;
                closest = distance;
                pickable = weapon.Value;
            }

            return found;
        }
    }
}
