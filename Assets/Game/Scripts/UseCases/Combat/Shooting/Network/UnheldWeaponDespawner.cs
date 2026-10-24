using Bw.Entities.Network.Ticks;
using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Combat.Abstractions.Inventory.Extensions;
using Bw.UseCases.Combat.Abstractions.Weapons;
using JetBrains.Lifetimes;
using Unity.Netcode;
using UnityEngine;

namespace Bw.UseCases.Combat.Shooting.Network
{
    public sealed class UnheldWeaponDespawner
    {
        private readonly INetworkTicks _ticks;
        private readonly ShootingWeaponConfig _config;
        private readonly NetworkObject _networkObject;

        public UnheldWeaponDespawner(
            Lifetime lifetime,
            IWeapon weapon,
            IWeaponCarriers carriers,
            INetworkTicks ticks,
            ShootingWeaponConfig config,
            NetworkObject networkObject)
        {
            _ticks = ticks;
            _config = config;
            _networkObject = networkObject;

            carriers.ViewUncarried(lifetime, weapon, DespawnLater);
        }

        private void DespawnLater(Lifetime uncarriedLifetime)
        {
            var despawnTick = _ticks.Current + Mathf.RoundToInt(_config.UnheldDespawnTime / _ticks.Duration);

            _ticks.Ticked(TickPhase.Default).Advise(uncarriedLifetime, tick =>
            {
                if (tick >= despawnTick)
                    _networkObject.Despawn();
            });
        }
    }
}
