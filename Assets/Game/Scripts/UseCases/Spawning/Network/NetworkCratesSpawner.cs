using Bw.Entities.Network;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Bw.UseCases.Spawning.Network
{
    public sealed class NetworkCratesSpawner
    {
        private readonly DiContainer _container;
        private readonly NetworkObject _prefab;
        private readonly Transform[] _spots;

        public NetworkCratesSpawner(
            Lifetime lifetime,
            INetworkHolder networkHolder,
            DiContainer container,
            NetworkObject prefab,
            Transform[] spots)
        {
            _container = container;
            _prefab = prefab;
            _spots = spots;

            networkHolder.NetworkManager.AdviseNotNull(lifetime, _ => SpawnAll());
        }

        private void SpawnAll() //TODO: разбитые ящики не возвращаются — к концу долгого матча на арене не остаётся укрытий
        {
            foreach (var spot in _spots)
                NetworkPrefabInstantiationHelper.Instantiate(_container, _prefab, spot.position, spot.rotation)
                    .Spawn(destroyWithScene: true);
        }
    }
}
