using System.Collections.Generic;
using Bw.Entities.Network;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using Zenject;

namespace Bw.UseCases.Spawning.Network
{
    public sealed class NetworkPlatformsSpawner
    {
        private readonly DiContainer _container;
        private readonly IReadOnlyList<MovingPlatformSpot> _spots;

        public NetworkPlatformsSpawner(
            Lifetime lifetime,
            INetworkHolder networkHolder,
            DiContainer container,
            MovingPlatformSpot[] spots)
        {
            _container = container;
            _spots = spots;

            networkHolder.NetworkManager.AdviseNotNull(lifetime, _ => SpawnAll());
        }

        private void SpawnAll()
        {
            for (var index = 0; index < _spots.Count; index++)
            {
                var spot = _spots[index];
                NetworkPrefabInstantiationHelper.Instantiate(_container, spot.Platform, spot.transform.position, spot.transform.rotation)
                    .Spawn(destroyWithScene: true);
            }
        }
    }
}
