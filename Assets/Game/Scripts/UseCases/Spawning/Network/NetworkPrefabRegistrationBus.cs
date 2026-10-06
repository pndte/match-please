using System;
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

        public NetworkPrefabRegistrationBus(Lifetime lifetime, DiContainer container, NetworkManager networkManager)
        {
            _lifetime = lifetime;
            _container = container;
            _networkManager = networkManager;
        }

        public void Initialize()
        {
            foreach (var networkPrefab in _networkManager.NetworkConfig.Prefabs.Prefabs)
            {
                var prefab = RequireWithoutOverride(networkPrefab);
                if (!_networkManager.PrefabHandler.AddHandler(prefab, new NetworkPrefabHandler(_container, prefab)))
                    throw new InvalidOperationException($"Network prefab '{prefab.name}' already has an instance handler.");

                _lifetime.OnTermination(() => _networkManager.PrefabHandler.RemoveHandler(prefab));
            }
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
