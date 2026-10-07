using Bw.Entities.Extensions;
using Bw.Entities.Network.Objects;
using Bw.Entities.Network.Ticks;
using Bw.UseCases.Character.Extensions;
using JetBrains.Lifetimes;
using Unity.Netcode;
using UnityEngine;

namespace Bw.UseCases.Character.Network
{
    public sealed class DeadCharacterDespawner
    {
        private readonly INetworkTicks _ticks;
        private readonly CharacterDeathConfig _config;
        private readonly NetworkObject _networkObject;

        public DeadCharacterDespawner(
            Lifetime lifetime,
            INetworkLifetimedObject networkLifetimed,
            IReadonlyCharacter character,
            INetworkTicks ticks,
            CharacterDeathConfig config,
            NetworkObject networkObject)
        {
            _ticks = ticks;
            _config = config;
            _networkObject = networkObject;

            networkLifetimed.SpawnedLifetime.WhenAlive(lifetime, spawnedLifetime =>
                character.State.WhenDead(spawnedLifetime, DespawnLater));
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
