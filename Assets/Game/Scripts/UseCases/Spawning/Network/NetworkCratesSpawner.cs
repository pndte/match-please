using System;
using Bw.Entities.Extensions;
using Bw.Entities.Network;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Weapon;
using Bw.UseCases.Combat.Weapon.Abstractions;
using Bw.UseCases.Mortality.Extensions;
using Bw.UseCases.Mortality.Network;
using Cysharp.Threading.Tasks;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Bw.UseCases.Spawning.Network
{
    public sealed class NetworkCratesSpawner
    {
        private readonly Lifetime _lifetime;
        private readonly DiContainer _container;
        private readonly CrateSpawnConfig _config;
        private readonly CrateSpots _spots;
        private readonly IWeaponKindRegistry _kinds;

        public NetworkCratesSpawner(
            Lifetime lifetime,
            INetworkHolder networkHolder,
            DiContainer container,
            CrateSpawnConfig config,
            CrateSpots spots,
            IWeaponKindRegistry kinds)
        {
            _lifetime = lifetime;
            _container = container;
            _config = config;
            _spots = spots;
            _kinds = kinds;

            networkHolder.NetworkManager.AdviseNotNull(lifetime, _ => SpawnAll());
        }

        private void SpawnAll()
        {
            foreach (var spot in _spots.All)
                SpawnAt(spot);
        }

        private void SpawnAt(Transform spot)
        {
            if (!_spots.IsFree(spot))
            {
                RetryLater(spot).Forget();
                return;
            }

            var crate = NetworkPrefabInstantiationHelper.Instantiate(_container, _config.Prefab, spot.position, spot.rotation);
            _kinds.AddLifetimed(crate.gameObject.Lifetime(), WeaponOf(crate), WeaponKind.Prop); //TODO: ящик создаёт спавнер ящиков, а не Armory, и в каталоге оружия его нет, поэтому описания у ящика нет: когда матч начнёт засчитывать броски (HitScorer), убийство ящиком бросит исключение в ленте убийств, пока ящику не дадут описание, строку в каталоге, иконку в WeaponIconsConfig и создание через Armory
            crate.Spawn(destroyWithScene: true);
            crate.GetComponent<ReadonlyMortalHolder>().Value.State.WhenDead(crate.gameObject.Lifetime(), _ => RespawnLater(spot).Forget());
        }

        private static IWeapon WeaponOf(NetworkObject crate) =>
            crate.TryGetComponent(out WeaponHolder holder)
                ? holder.Value
                : throw new System.InvalidOperationException($"'{crate.name}' has no WeaponHolder after its prefab install: the crate config lists a prefab that is not a prop.");

        private async UniTaskVoid RespawnLater(Transform spot)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(_config.RespawnDelay), cancellationToken: _lifetime);
            SpawnAt(spot);
        }

        private async UniTaskVoid RetryLater(Transform spot)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(_config.RetryDelay), cancellationToken: _lifetime);
            SpawnAt(spot);
        }
    }
}
