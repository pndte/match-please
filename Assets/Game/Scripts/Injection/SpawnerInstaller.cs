using System;
using Bw.Entities.Network;
using Bw.Entities.Network.Characters;
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
        [SerializeField] private CharacterCatalog _characters;
        [SerializeField] private NetworkObject _fists;
        [SerializeField] private NetworkObject[] _weapons;
        [SerializeField] private Transform[] _spawnPoints;
        [SerializeField] private float _spawnRandomOffset = 2f;

        [Header("Match")]
        [SerializeField] private NetworkObject _match;

        [Header("Crates")]
        [SerializeField] private CrateSpawnConfig _crates;
        [SerializeField] private Transform[] _crateSpots = Array.Empty<Transform>();

        public override void InstallBindings()
        {
            Container.BindInterfacesTo<NetworkClientCollection>().AsSingle()
                .WithArguments(NetworkManager.Singleton, _characters).NonLazy();
            Container.Bind<IPlayerCollection>().To<PlayerCollection>().AsSingle();
            Container.BindInterfacesTo<NetworkPlayerCollection>().AsSingle();

            if (_runtimeSettings.CurrentPeerType != PeerType.Server)
                return;

            RequireCharacters(NetworkManager.Singleton.NetworkConfig.Prefabs, _characters);
            RequireNetworkPrefab(NetworkManager.Singleton.NetworkConfig.Prefabs, _fists);
            RequirePrefabs(NetworkManager.Singleton.NetworkConfig.Prefabs, _weapons, "weapon");
            Container.BindInterfacesTo<NetworkCharactersSpawner>().AsSingle().WithArguments(
                new NetworkCharactersSpawner.Data
                {
                    SpawnPoints = _spawnPoints,
                    SpawnRandomOffset = _spawnRandomOffset,
                    Characters = _characters,
                    FistsPrefab = _fists,
                    WeaponPrefabs = _weapons,
                });
            Container.Bind<CharacterRespawner>().AsSingle().NonLazy();
            RequireNetworkPrefab(NetworkManager.Singleton.NetworkConfig.Prefabs, _match);
            Container.Bind<NetworkMatchSpawner>().AsSingle().WithArguments(_match).NonLazy();

            if (_crateSpots.Length > 0)
                BindCrates(NetworkManager.Singleton.NetworkConfig.Prefabs);
        }

        private void BindCrates(NetworkPrefabs registered)
        {
            if (_crates == null)
                throw new InvalidOperationException("The spawner has crate spots but no crate config: assign Resources/Crates/CrateSpawnConfig.");

            RequireNetworkPrefab(registered, _crates.Prefab);
            Container.Bind<NetworkCratesSpawner>().AsSingle().WithArguments(_crates, _crateSpots).NonLazy();
        }

        private static void RequireCharacters(NetworkPrefabs registered, CharacterCatalog characters)
        {
            CharacterCatalogValidation.Require(characters);
            foreach (var character in characters.Characters)
                RequireNetworkPrefab(registered, character.Prefab);
        }

        private static void RequirePrefabs(NetworkPrefabs registered, NetworkObject[] prefabs, string kind)
        {
            if (prefabs.Length == 0)
                throw new InvalidOperationException($"The spawner has no {kind} prefabs: add at least one to its list.");

            foreach (var prefab in prefabs)
                RequireNetworkPrefab(registered, prefab);
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
