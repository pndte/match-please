using System;
using Bw.Entities.Extensions;
using Bw.Entities.Network;
using Bw.Entities.Physics;
using Bw.Entities.Physics.Extensions;
using Bw.UseCases.Mortality.Extensions;
using Bw.UseCases.Mortality.Network;
using Cysharp.Threading.Tasks;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using Zenject;

namespace Bw.UseCases.Spawning.Network
{
    public sealed class NetworkCratesSpawner
    {
        private readonly Lifetime _lifetime;
        private readonly DiContainer _container;
        private readonly CrateSpawnConfig _config;
        private readonly Transform[] _spots;
        private readonly PhysicsWorld _world;

        public NetworkCratesSpawner(
            Lifetime lifetime,
            INetworkHolder networkHolder,
            DiContainer container,
            CrateSpawnConfig config,
            Transform[] spots,
            IPhysicsSpace space)
        {
            _lifetime = lifetime;
            _container = container;
            _config = config;
            _spots = spots;
            _world = space.World;

            networkHolder.NetworkManager.AdviseNotNull(lifetime, _ => SpawnAll());
        }

        private void SpawnAll()
        {
            foreach (var spot in _spots)
                SpawnAt(spot);
        }

        private void SpawnAt(Transform spot)
        {
            if (!IsFree(spot))
            {
                RetryLater(spot).Forget();
                return;
            }

            var crate = NetworkPrefabInstantiationHelper.Instantiate(_container, _config.Prefab, spot.position, spot.rotation);
            crate.Spawn(destroyWithScene: true);
            crate.GetComponent<ReadonlyMortalHolder>().Value.State.WhenDead(crate.gameObject.Lifetime(), _ => RespawnLater(spot).Forget());
        }

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

        private bool IsFree(Transform spot)
        {
            var box = _config.Prefab.GetComponent<BodyBox>();
            var rotation = spot.rotation;
            var center = (Vector2)spot.position + (Vector2)(rotation * box.WorldOffset());
            var pose = new PhysicsTransform(center, new PhysicsRotate(rotation.eulerAngles.z * Mathf.Deg2Rad));
            var shape = PolygonGeometry.CreateBox(box.WorldSize() - Vector2.one * (PhysicsSlop.Linear * 2f), 0f, pose, false);
            return _world.IsFree(shape, new PhysicsQuery.QueryFilter(PhysicsMask.All, _config.Blockers.AsPhysicsMask()));
        }
    }
}
