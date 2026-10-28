using System;
using Bw.Entities.Network;
using Bw.Entities.Network.Objects;
using Bw.Entities.Pool;
using Bw.Entities.Pool.GameObjects;
using Bw.UseCases.Combat.Weapon.Abstractions;
using JetBrains.Lifetimes;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Bw.UseCases.Spawning.Network
{
    public sealed class NetworkPrefabHandlerFactory
    {
        private readonly Lifetime _lifetime;
        private readonly DiContainer _container;
        private readonly IPools _pools;
        private readonly IRuntimeSettings _runtimeSettings;
        private readonly WeaponPrefabKinds _prefabKinds;
        private readonly IWeaponKindRegistry _kinds;

        public NetworkPrefabHandlerFactory(
            Lifetime lifetime,
            DiContainer container,
            IPools pools,
            IRuntimeSettings runtimeSettings,
            WeaponPrefabKinds prefabKinds,
            IWeaponKindRegistry kinds)
        {
            _lifetime = lifetime;
            _container = container;
            _pools = pools;
            _runtimeSettings = runtimeSettings;
            _prefabKinds = prefabKinds;
            _kinds = kinds;
        }

        public INetworkPrefabInstanceHandler Create(GameObject prefab) =>
            _runtimeSettings.CurrentPeerType switch
            {
                PeerType.Server => ServerHandler(prefab),
                PeerType.Client => ClientHandler(prefab),
                _ => throw new ArgumentOutOfRangeException(nameof(_runtimeSettings.CurrentPeerType), _runtimeSettings.CurrentPeerType, null),
            };

        private INetworkPrefabInstanceHandler ServerHandler(GameObject prefab) =>
            IsPooled(prefab) ? new PoolSpawnedNetworkPrefabHandler(prefab) : new NetworkPrefabHandler(_container, prefab);

        private INetworkPrefabInstanceHandler ClientHandler(GameObject prefab)
        {
            INetworkPrefabInstanceHandler handler = IsPooled(prefab)
                ? new PooledNetworkPrefabHandler(_lifetime, _pools.For<NetworkObject>(prefab))
                : new NetworkPrefabHandler(_container, prefab);
            return _prefabKinds.ByPrefab.TryGetValue(prefab, out var kind)
                ? new KindRegisteringPrefabHandler(_lifetime, handler, kind, _kinds)
                : handler;
        }

        private static bool IsPooled(GameObject prefab) =>
            prefab.TryGetComponent<PooledNetworkObject>(out _);
    }
}
