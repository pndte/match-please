using Bw.Entities.Extensions;
using Bw.Entities.Network.Objects;
using Bw.Entities.Network.Ticks;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using Unity.Netcode;
using UnityEngine;

namespace Bw.UseCases.Shooting.Weapon.Network
{
    public sealed class UnheldWeaponDespawner
    {
        private readonly INetworkTicks _ticks;
        private readonly ShootingWeaponConfig _config;
        private readonly NetworkObject _networkObject;

        public UnheldWeaponDespawner(
            Lifetime lifetime,
            INetworkLifetimedObject networkLifetimedObject,
            IWeaponHold hold,
            INetworkTicks ticks,
            ShootingWeaponConfig config,
            NetworkObject networkObject)
        {
            _ticks = ticks;
            _config = config;
            _networkObject = networkObject;

            networkLifetimedObject.SpawnedLifetime.WhenAlive(lifetime, spawnedLifetime =>
                hold.HeldLifetime.View(spawnedLifetime, (holdStateLifetime, heldLifetime) =>
                {
                    if (!heldLifetime.IsAlive)
                        DespawnLater(holdStateLifetime);
                }));
        }

        private void DespawnLater(Lifetime unheldLifetime)
        {
            var despawnTick = _ticks.Current + Mathf.RoundToInt(_config.UnheldDespawnTime / _ticks.Duration);

            _ticks.Ticked(TickPhase.Default).Advise(unheldLifetime, tick =>
            {
                if (tick >= despawnTick)
                    _networkObject.Despawn();
            });
        }
    }
}
