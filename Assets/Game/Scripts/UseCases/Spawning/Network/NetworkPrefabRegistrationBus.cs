using System;
using JetBrains.Lifetimes;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Bw.UseCases.Spawning.Network
{
    public sealed class NetworkPrefabRegistrationBus : IInitializable
    {
        private readonly Lifetime _lifetime;
        private readonly NetworkManager _networkManager;
        private readonly NetworkPrefabHandlerFactory _handlers;

        public NetworkPrefabRegistrationBus(Lifetime lifetime, NetworkManager networkManager, NetworkPrefabHandlerFactory handlers)
        {
            _lifetime = lifetime;
            _networkManager = networkManager;
            _handlers = handlers;
        }

        public void Initialize()
        {
            foreach (var networkPrefab in _networkManager.NetworkConfig.Prefabs.Prefabs)
            {
                var prefab = RequireWithoutOverride(networkPrefab);
                if (!_networkManager.PrefabHandler.AddHandler(prefab, _handlers.Create(prefab)))
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
