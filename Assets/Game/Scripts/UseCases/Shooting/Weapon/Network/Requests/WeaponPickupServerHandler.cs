using System.Collections.Generic;
using Bw.Entities;
using Bw.Entities.Network.Variables;
using Bw.UseCases.Mortality;
using Bw.UseCases.Mortality.Extensions;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using Bw.UseCases.Shooting.Weapon.Extensions;
using Bw.UseCases.Shooting.Weapon.Pickup;
using Bw.UseCases.Shooting.Weapon.Pickup.Extensions;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Shooting.Weapon.Network.Requests
{
    public sealed class WeaponPickupServerHandler
    {
        private readonly IReadonlyMortal _character;
        private readonly IWeaponInventories _inventories;
        private readonly WeaponPickupConfig _config;
        private readonly WeaponPickupNetworkConfig _networkConfig;
        private readonly Transform _body;
        private readonly ContactFilter2D _filter;
        private readonly List<Collider2D> _found = new(8);

        public WeaponPickupServerHandler(
            Lifetime lifetime,
            IReadonlyMortal character,
            INetRequestReceiver<WeaponPickup> pickup,
            IWeaponInventories inventories,
            WeaponPickupConfig config,
            WeaponPickupNetworkConfig networkConfig,
            Transform body)
        {
            _character = character;
            _inventories = inventories;
            _config = config;
            _networkConfig = networkConfig;
            _body = body;
            _filter = new ContactFilter2D
            {
                useLayerMask = true,
                layerMask = config.WeaponMask,
                useTriggers = false
            };

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

            Physics2D.OverlapCircle(point, _config.HoverRadius, _filter, _found); //TODO: точку клиент берёт со своей картинки, а летящее оружие он видит с отставанием интерполяции — подбор на лету может промахнуться; надёжнее, если запрос назовёт само оружие (id ↔ оружие на сетевой границе)
            for (var index = 0; index < _found.Count; index++)
            {
                var shape = _found[index];
                if (!shape.TryGetComponent<IHolder<IWeapon>>(out var holder) || _inventories.Carriers.ContainsKey(holder.Value))
                    continue;

                var distance = Vector2.Distance(point, shape.ClosestPoint(point));
                if (distance >= closest || !_config.IsPickable(_body.position, shape.bounds.center, distance, _networkConfig.ReachSlack))
                    continue;

                found = true;
                closest = distance;
                pickable = holder.Value;
            }

            return found;
        }
    }
}
