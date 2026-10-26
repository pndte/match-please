using Bw.Entities.Network.Rollback;
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
        private readonly IWorldStep _worldStep;
        private readonly ShootingWeaponConfig _config;
        private readonly NetworkObject _networkObject;

        public UnheldWeaponDespawner( //TODO: чрезвычайное к-во параметров
            Lifetime lifetime,
            IWeapon weapon,
            IReadonlyWeaponCarriers carriers,
            INetworkTicks ticks,
            IWorldStep worldStep,
            ShootingWeaponConfig config,
            NetworkObject networkObject)
        {
            _ticks = ticks;
            _worldStep = worldStep;
            _config = config;
            _networkObject = networkObject;

            carriers.ViewUncarried(lifetime, weapon, DespawnLater);
        }

        private void DespawnLater(Lifetime uncarriedLifetime)
        {
            var despawnTick = _ticks.Current + Mathf.RoundToInt(_config.UnheldDespawnTime / _ticks.Duration);

            _worldStep.Stepped(TickPhase.Default).Advise(uncarriedLifetime, tick =>
            {
                if (tick >= despawnTick)
                    _networkObject.Despawn();
            });
        }
    }
}
