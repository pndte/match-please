using System;
using Bw.Entities.Network;
using Bw.Entities.Network.Objects;
using Bw.Entities.Pool.GameObjects;
using JetBrains.Lifetimes;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Bw.UseCases.Spawning.Network
{
    public sealed class NetworkPrefabRegistrationBus : IInitializable //todo: refactor
    {
        private readonly Lifetime _lifetime;
        private readonly DiContainer _container;
        private readonly NetworkManager _networkManager;
        private readonly IPrefabPools _pools;
        private readonly IRuntimeSettings _runtimeSettings;

        public NetworkPrefabRegistrationBus(Lifetime lifetime, DiContainer container, NetworkManager networkManager, IPrefabPools pools, IRuntimeSettings runtimeSettings)
        {
            _lifetime = lifetime;
            _container = container;
            _networkManager = networkManager;
            _pools = pools;
            _runtimeSettings = runtimeSettings;
        }

        public void Initialize()
        {
            foreach (var networkPrefab in _networkManager.NetworkConfig.Prefabs.Prefabs)
            {
                var prefab = RequireWithoutOverride(networkPrefab);
                if (!_networkManager.PrefabHandler.AddHandler(prefab, HandlerFor(prefab)))
                    throw new InvalidOperationException($"Network prefab '{prefab.name}' already has an instance handler.");

                _lifetime.OnTermination(() => _networkManager.PrefabHandler.RemoveHandler(prefab));
            }
        }

        private INetworkPrefabInstanceHandler HandlerFor(GameObject prefab)
        {
            if (!prefab.TryGetComponent<PooledNetworkObject>(out _))
                return new NetworkPrefabHandler(_container, prefab);

            return _runtimeSettings.CurrentPeerType switch
            {
                PeerType.Server => new PoolSpawnedNetworkPrefabHandler(prefab),
                PeerType.Client => new PooledNetworkPrefabHandler(_lifetime, _pools.For<NetworkObject>(prefab)),
                _ => throw new ArgumentOutOfRangeException(nameof(_runtimeSettings.CurrentPeerType), _runtimeSettings.CurrentPeerType, null),
            };
        }

        private static GameObject RequireWithoutOverride(NetworkPrefab networkPrefab)
        {
            if (networkPrefab.Override != NetworkPrefabOverride.None)
                throw new NotSupportedException(
                    $"Network prefab {networkPrefab} has an override: handlers instantiate the listed prefab itself through Zenject.");

            return networkPrefab.Prefab;
        }
    }
}
