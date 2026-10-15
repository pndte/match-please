using System;
using Bw.Entities.Network;
using Bw.CoreAdapters.Characters;
using Bw.CoreAdapters.Weapons;
using Bw.Entities.Players;
using Bw.UseCases.Clients.Network;
using Bw.UseCases.Players.Network;
using Bw.UseCases.Shooting.Weapon;
using Bw.UseCases.Shooting.Weapon.Abstractions;
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
        [Inject] private CharacterCatalog _characters;
        [Inject] private WeaponCatalog _weaponCatalog;

        [Header("Spawn Configuration")]
        [SerializeField] private WeaponDescription _fists;
        [SerializeField] private WeaponDescription[] _weapons;
        [SerializeField] private Transform[] _spawnPoints;
        [SerializeField] private float _spawnRandomOffset = 2f;

        [Header("Match")]
        [SerializeField] private NetworkObject _match;

        [Header("Crates")]
        [SerializeField] private CrateSpawnConfig _crates;
        [SerializeField] private Transform[] _crateSpots = Array.Empty<Transform>();

        public override void InstallBindings()
        {
            Container.BindInterfacesTo<NetworkClientCollection>().AsSingle().WithArguments(NetworkManager.Singleton).NonLazy();
            Container.Bind<IPlayerCollection>().To<PlayerCollection>().AsSingle();
            Container.BindInterfacesTo<NetworkPlayerCollection>().AsSingle();

            if (_runtimeSettings.CurrentPeerType != PeerType.Server)
                return;

            RequireCharacterPrefabs(NetworkManager.Singleton.NetworkConfig.Prefabs, _characters);
            RequireWeaponPrefabs(NetworkManager.Singleton.NetworkConfig.Prefabs, _weaponCatalog);
            RequireLoadout();
            Container.Bind(typeof(IArmory), typeof(IReadonlyWeaponDescriptionRegistry)).To<Armory>().AsSingle();
            Container.Bind<StartingLoadout>().AsSingle().WithArguments(_fists, _weapons);
            Container.BindInterfacesTo<NetworkCharactersSpawner>().AsSingle().WithArguments(
                new NetworkCharactersSpawner.Data
                {
                    SpawnPoints = _spawnPoints,
                    SpawnRandomOffset = _spawnRandomOffset,
                    Characters = _characters,
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

        private void RequireLoadout()
        {
            RequireListedWeapon(_fists);
            if (_weapons.Length == 0)
                throw new InvalidOperationException("The spawner has no weapons to arm characters with: add at least one description to its list.");

            foreach (var weapon in _weapons)
                RequireListedWeapon(weapon);
        }

        private void RequireListedWeapon(WeaponDescription weapon)
        {
            if (weapon == null)
                throw new InvalidOperationException("The spawner has an empty weapon slot: assign a description from Assets/Resources/Weapons or remove the slot.");

            if (!_weaponCatalog.Contains(weapon))
                throw new InvalidOperationException(
                    $"The spawner arms characters with '{weapon.name}', which the weapon catalog '{_weaponCatalog.name}' lacks: add a row with its prefab.");
        }

        private static void RequireCharacterPrefabs(NetworkPrefabs registered, CharacterCatalog characters)
        {
            foreach (var character in characters.Characters)
                RequireNetworkPrefab(registered, character.Prefab);
        }

        private static void RequireWeaponPrefabs(NetworkPrefabs registered, WeaponCatalog weapons)
        {
            foreach (var weapon in weapons.Weapons)
                RequireNetworkPrefab(registered, weapon.Prefab);
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
