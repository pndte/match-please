using System;
using Bw.Entities;
using Bw.Entities.Extensions;
using Bw.Entities.Network;
using Bw.Entities.Players;
using Bw.UseCases.Character;
using Bw.UseCases.Character.Extensions;
using Bw.UseCases.Character.Network;
using Bw.UseCases.Shooting.Weapon;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using Unity.Netcode;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace Bw.UseCases.Spawning.Network
{
    public class NetworkCharactersSpawner : ICharacterSpawner //todo: refactor
    {
        public struct Data
        {
            public Transform[] SpawnPoints;
            public float SpawnRandomOffset;
            public NetworkObject[] CharacterPrefabs;
            public NetworkObject FistsPrefab;
            public NetworkObject[] WeaponPrefabs;
        }

        private readonly Data _data;
        private readonly ICharacterRegistry _characterRegistry;
        private readonly IGameObjectByCharacterCollection _gameObjectByCharacterCollection;
        private readonly IClientPlayerCollection _clientPlayerCollection;
        private readonly IWeaponInventories _inventories;
        private readonly DiContainer _container;
        private int _nextSpawnPointIndex;
        private int _weaponNameCounter;

        private NetworkCharactersSpawner(
            Lifetime lifetime,
            IPlayerCollection players,
            ICharacterRegistry characterRegistry,
            IGameObjectByCharacterCollection gameObjectByCharacterCollection,
            IClientPlayerCollection clientPlayerCollection,
            IWeaponInventories inventories,
            DiContainer container,
            Data data)
        {
            _characterRegistry = characterRegistry;
            _gameObjectByCharacterCollection = gameObjectByCharacterCollection;
            _clientPlayerCollection = clientPlayerCollection;
            _inventories = inventories;
            _container = container;
            _data = data;

            players.View(lifetime, (playerLifetime, player) =>
                SpawnCharacterFor(playerLifetime, player));
        }

        public IReadonlyCharacter SpawnCharacterFor(Lifetime lifetime, IPlayer player)
        {
            if (!_clientPlayerCollection.ByClient.TryGetLeft(player, out var client)) //TODO: боты — спавнить персонажа и без клиента
                throw new InvalidOperationException(
                    "The player has no client: the network spawner spawns characters only for network players.");

            var spawnPosition = GetSpawnPosition();
            var spawnRotation = Quaternion.identity;

            var characterPrefab = RandomCharacterPrefab();
            var characterObject = NetworkPrefabInstantiationHelper.Instantiate(
                _container, characterPrefab, spawnPosition, spawnRotation);

            characterObject.SpawnAsPlayerObject(client.Id, destroyWithScene: true);

            var characterHolder = RequireComponent<ReadonlyCharacterHolder>(characterObject.gameObject);
            var characterLifetime = characterHolder.gameObject.Lifetime();
            var character = characterHolder.Value;

            _gameObjectByCharacterCollection.AddLifetimed(
                characterLifetime, character, characterObject.gameObject);
            _characterRegistry.PlayerByCharacter.AddLifetimed(characterLifetime, character, player);

            var characterContext = RequireComponent<GameObjectContext>(characterObject.gameObject);
            var characterControlledBy = characterContext.Container.Resolve<IControlledBy>();
            var characterOwnership = characterContext.Container.Resolve<IOwnershipController>();
            characterOwnership.AddOwner(characterLifetime, player);
            character.State.WhenAlive(characterLifetime, aliveLifetime => characterControlledBy.Set(aliveLifetime, player));

            ArmCharacter(character, characterLifetime, characterObject.transform.position, spawnRotation);

            Debug.Log($"[PlayerSpawner] Character '{characterPrefab.name}' spawned for client {client.Id} at {spawnPosition}");
            return character;
        }

        private void ArmCharacter(
            IReadonlyCharacter character,
            Lifetime characterLifetime,
            Vector3 position,
            Quaternion rotation)
        {
            var gun = WeaponOf(SpawnWeapon(RandomWeaponPrefab(), position, rotation));

            character.State.WhenAlive(characterLifetime, aliveLifetime =>
            {
                var fists = SpawnWeapon(_data.FistsPrefab, position, rotation);
                aliveLifetime.OnTermination(() => Despawn(fists));
                _inventories.Give(aliveLifetime, character, WeaponSlot.Fists, WeaponOf(fists));
                _inventories.Give(aliveLifetime, character, WeaponSlot.Primary, gun);
                _inventories.Draw(gun);
            });
        }

        private NetworkObject SpawnWeapon(NetworkObject prefab, Vector3 position, Quaternion rotation)
        {
            var weaponObject = NetworkPrefabInstantiationHelper.Instantiate(_container, prefab, position, rotation);
            weaponObject.name += $", {_weaponNameCounter++}";
            weaponObject.Spawn(destroyWithScene: true);
            return weaponObject;
        }

        private static IWeapon WeaponOf(NetworkObject weaponObject) =>
            RequireComponent<WeaponHolder>(weaponObject.gameObject).Value;

        private static void Despawn(NetworkObject weaponObject)
        {
            if (weaponObject.IsSpawned)
                weaponObject.Despawn();
        }

        private NetworkObject RandomCharacterPrefab() =>
            _data.CharacterPrefabs[Random.Range(0, _data.CharacterPrefabs.Length)];

        private NetworkObject RandomWeaponPrefab() =>
            _data.WeaponPrefabs[Random.Range(0, _data.WeaponPrefabs.Length)];

        private Vector3 GetSpawnPosition()
        {
            var basePosition = _data.SpawnPoints is { Length: > 0 }
                ? _data.SpawnPoints[_nextSpawnPointIndex++ % _data.SpawnPoints.Length].position
                : Vector3.zero;

            if (_data.SpawnRandomOffset <= 0f)
                return basePosition;

            return basePosition + new Vector3(
                Random.Range(-_data.SpawnRandomOffset, _data.SpawnRandomOffset),
                0f,
                Random.Range(-_data.SpawnRandomOffset, _data.SpawnRandomOffset));
        }

        private static T RequireComponent<T>(GameObject gameObject) where T : Component
        {
            if (gameObject.TryGetComponent(out T component))
                return component;

            throw new InvalidOperationException(
                $"Component {typeof(T).Name} is missing on '{gameObject.name}' after prefab install.");
        }
    }
}
