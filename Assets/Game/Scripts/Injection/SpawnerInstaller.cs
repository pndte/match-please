using System;
using Bw.Entities.Network;
using Bw.Entities.Players;
using Bw.UseCases.Clients.Network;
using Bw.UseCases.Players.Network;
using Bw.UseCases.Spawning;
using Bw.UseCases.Spawning.Network;
using Setup;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Bw.Injection
{
    public class SpawnerInstaller : MonoInstaller
    {
        [Inject] private IRuntimeSettings _runtimeSettings;

        [Header("Spawn Configuration")]
        [SerializeField] private NetworkObject[] _characters;
        [SerializeField] private NetworkObject _weapon;
        [SerializeField] private Transform[] _spawnPoints;
        [SerializeField] private float _spawnRandomOffset = 2f;

        public override void InstallBindings()
        {
            Container.BindInterfacesTo<NetworkClientCollection>().AsSingle()
                .WithArguments(NetworkManager.Singleton).NonLazy();
            Container.Bind<IPlayerCollection>().To<PlayerCollection>().AsSingle();
            Container.BindInterfacesTo<NetworkPlayerCollection>().AsSingle();

            if (_runtimeSettings.CurrentPeerType != PeerType.Server)
                return;

            RequireCharacterPrefabs(NetworkManager.Singleton.NetworkConfig.Prefabs);
            RequireNetworkPrefab(NetworkManager.Singleton.NetworkConfig.Prefabs, _weapon);
            Container.BindInterfacesTo<NetworkCharactersSpawner>().AsSingle().WithArguments(
                new NetworkCharactersSpawner.Data
                {
                    SpawnPoints = _spawnPoints,
                    SpawnRandomOffset = _spawnRandomOffset,
                    CharacterPrefabs = _characters,
                    WeaponPrefab = _weapon,
                });
            Container.Bind<CharacterRespawner>().AsSingle().NonLazy();
        }

        private void RequireCharacterPrefabs(NetworkPrefabs registered)
        {
            if (_characters.Length == 0)
                throw new InvalidOperationException("The spawner has no character prefabs: add at least one to its list.");

            foreach (var character in _characters)
                RequireNetworkPrefab(registered, character);
        }

        private static void RequireNetworkPrefab(NetworkPrefabs registered, NetworkObject prefab)
        {
            if (prefab == null)
                throw new InvalidOperationException("The spawner has an empty prefab slot: assign a prefab or remove the slot.");

            if (!registered.Contains(prefab.gameObject))
                throw new InvalidOperationException(
                    $"Prefab '{prefab.name}' is not in the NGO prefab list, so clients can't create it: NGO adds every prefab with a NetworkObject to Assets/Game/DefaultNetworkPrefabs.asset.");
        }
    }
}
