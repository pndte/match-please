using System;
using Bw.Entities.Extensions;
using Bw.Entities.Network;
using Bw.UseCases.Mortality.Extensions;
using Bw.UseCases.Mortality.Network;
using Cysharp.Threading.Tasks;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using UnityEngine;
using Zenject;

namespace Bw.UseCases.Spawning.Network
{
    public sealed class NetworkCratesSpawner
    {
        private readonly Lifetime _lifetime;
        private readonly DiContainer _container;
        private readonly CrateSpawnConfig _config;
        private readonly Transform[] _spots;

        public NetworkCratesSpawner(
            Lifetime lifetime,
            INetworkHolder networkHolder,
            DiContainer container,
            CrateSpawnConfig config,
            Transform[] spots)
        {
            _lifetime = lifetime;
            _container = container;
            _config = config;
            _spots = spots;

            networkHolder.NetworkManager.AdviseNotNull(lifetime, _ => SpawnAll());
        }

        private void SpawnAll()
        {
            foreach (var spot in _spots)
                SpawnAt(spot);
        }

        private void SpawnAt(Transform spot) //TODO: ящик возвращается на своё место, не проверяя, свободно ли оно: если там стоит игрок или другой ящик, они окажутся друг в друге
        {
            var crate = NetworkPrefabInstantiationHelper.Instantiate(_container, _config.Prefab, spot.position, spot.rotation);
            crate.Spawn(destroyWithScene: true);
            crate.GetComponent<ReadonlyMortalHolder>().Value.State.WhenDead(crate.gameObject.Lifetime(), _ => RespawnLater(spot).Forget());
        }

        private async UniTaskVoid RespawnLater(Transform spot)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(_config.RespawnDelay), cancellationToken: _lifetime);
            SpawnAt(spot);
        }
    }
}
