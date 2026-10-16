using Bw.Entities;
using Bw.Entities.Network.Variables;
using Bw.Entities.Physics.Extensions;
using Bw.UseCases.Mortality;
using Bw.UseCases.Mortality.Extensions;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using Bw.UseCases.Shooting.Weapon.Extensions;
using Bw.UseCases.Shooting.Weapon.Pickup;
using Bw.UseCases.Shooting.Weapon.Pickup.Extensions;
using JetBrains.Lifetimes;
using Unity.Collections;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.UseCases.Shooting.Weapon.Network.Requests
{
    public sealed class WeaponPickupServerHandler
    {
        private readonly IReadonlyMortal _character;
        private readonly IWeaponInventories _inventories;
        private readonly WeaponPickupConfig _config;
        private readonly WeaponPickupNetworkConfig _networkConfig;
        private readonly PhysicsBody _body;
        private readonly PhysicsQuery.QueryFilter _filter;

        public WeaponPickupServerHandler(
            Lifetime lifetime,
            IReadonlyMortal character,
            INetRequestReceiver<WeaponPickup> pickup,
            IWeaponInventories inventories,
            WeaponPickupConfig config,
            WeaponPickupNetworkConfig networkConfig,
            PhysicsBody body)
        {
            _character = character;
            _inventories = inventories;
            _config = config;
            _networkConfig = networkConfig;
            _body = body;
            _filter = new PhysicsQuery.QueryFilter(PhysicsMask.All, config.WeaponMask.AsPhysicsMask());

            character.State.WhenAlive(lifetime, aliveLifetime =>
                pickup.Received.Advise(aliveLifetime, request => PickUp(request.Point)));
        }

        private void PickUp(Vector2 point)
        {
            var inventory = _inventories.InventoryOf(_character);
            if (inventory.Switchable() && TryFindPickable(point, out var weapon))
                inventory.PickUpSwapping(weapon);
        }

        private bool TryFindPickable(Vector2 point, out IWeapon pickable)
        {
            var found = false;
            var closest = float.MaxValue;
            pickable = default;

            var hover = new CircleGeometry { center = point, radius = _config.HoverRadius };
            using var near = _body.world.OverlapGeometry(hover, _filter, Allocator.Temp); //TODO: точку клиент берёт со своей картинки, а летящее оружие он видит с отставанием интерполяции — подбор на лету может промахнуться; надёжнее, если запрос назовёт само оружие (id ↔ оружие на сетевой границе)
            for (var index = 0; index < near.Length; index++)
            {
                var shape = near[index].shape;
                if (!shape.TryGetComponent<IHolder<IWeapon>>(out var weapon) || _inventories.Carriers.ContainsKey(weapon.Value))
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
