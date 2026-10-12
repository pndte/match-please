using Bw.Entities.Extensions;
using Bw.Entities.Network.Objects;
using Bw.Entities.Network.Ticks;
using Bw.UseCases.Mortality.Extensions;
using JetBrains.Lifetimes;
using Unity.Netcode;
using UnityEngine;

namespace Bw.UseCases.Mortality.Network
{
    public sealed class DeadDespawner
    {
        private readonly INetworkTicks _ticks;
        private readonly DeathConfig _config;
        private readonly NetworkObject _networkObject;

        public DeadDespawner(
            Lifetime lifetime,
            INetworkLifetimedObject networkLifetimed,
            IReadonlyMortal mortal,
            INetworkTicks ticks,
            DeathConfig config,
            NetworkObject networkObject)
        {
            _ticks = ticks;
            _config = config;
            _networkObject = networkObject;

            networkLifetimed.SpawnedLifetime.WhenAlive(lifetime, spawnedLifetime =>
                mortal.State.WhenDead(spawnedLifetime, DespawnLater));
        }

        private void DespawnLater(Lifetime deadLifetime)
        {
            var despawnTick = _ticks.Current + Mathf.RoundToInt(_config.CorpseTime / _ticks.Duration);

            _ticks.Ticked(TickPhase.Default).Advise(deadLifetime, tick =>
            {
                if (tick >= despawnTick)
                    _networkObject.Despawn();
            });
        }
    }
}
