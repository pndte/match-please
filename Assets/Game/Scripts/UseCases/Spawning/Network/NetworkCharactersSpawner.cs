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
            public NetworkObject WeaponPrefab;
        }

        private readonly Data _data;
        private readonly ICharacterRegistry _characterRegistry;
        private readonly IGameObjectByCharacterCollection _gameObjectByCharacterCollection;
        private readonly IClientPlayerCollection _clientPlayerCollection;
        private readonly IHeldWeaponCollection _heldWeapons;
        private readonly DiContainer _container;
        private int _nextSpawnPointIndex;
        private int _weaponNameCounter;

        private NetworkCharactersSpawner(
            Lifetime lifetime,
            IPlayerCollection players,
            ICharacterRegistry characterRegistry,
            IGameObjectByCharacterCollection gameObjectByCharacterCollection,
            IClientPlayerCollection clientPlayerCollection,
            IHeldWeaponCollection heldWeapons,
            DiContainer container,
            Data data)
        {
            _characterRegistry = characterRegistry;
            _gameObjectByCharacterCollection = gameObjectByCharacterCollection;
            _clientPlayerCollection = clientPlayerCollection;
            _heldWeapons = heldWeapons;
            _container = container;
            _data = data;

            players.View(lifetime, (playerLifetime, player) =>
                SpawnCharacterFor(playerLifetime, player));
        }

        public ICharacter SpawnCharacterFor(Lifetime lifetime, IPlayer player)
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

            var characterHolder = RequireComponent<CharacterHolder>(characterObject.gameObject);
            var characterLifetime = characterHolder.gameObject.Lifetime();
            var character = characterHolder.Value;

            _gameObjectByCharacterCollection.AddLifetimed(
                characterLifetime, character, characterObject.gameObject);
            _characterRegistry.PlayerByCharacter.AddLifetimed(characterLifetime, character, player);

            var characterContext = RequireComponent<GameObjectContext>(characterObject.gameObject);
            var characterControlledBy = characterContext.Container.Resolve<IControlledBy>();
            var characterOwnership = characterContext.Container.Resolve<IOwnershipController>();
            characterOwnership.AddOwner(characterLifetime, player);
            characterControlledBy.Set(characterLifetime, player);

            SpawnWeaponFor(character, characterLifetime, characterObject.transform.position, spawnRotation);

            Debug.Log($"[PlayerSpawner] Character '{characterPrefab.name}' spawned for client {client.Id} at {spawnPosition}");
            return character;
        }

        private void SpawnWeaponFor(
            ICharacter character,
            Lifetime characterLifetime,
            Vector3 position,
            Quaternion rotation)
        {
            var weaponObject = NetworkPrefabInstantiationHelper.Instantiate(
                _container, _data.WeaponPrefab, position, rotation);
            weaponObject.name += $", {_weaponNameCounter++}";
            weaponObject.Spawn(destroyWithScene: true);

            var weapon = RequireComponent<WeaponHolder>(weaponObject.gameObject).Value;
            character.State.WhenAlive(characterLifetime, aliveLifetime =>
                _heldWeapons.ByCharacter.AddLifetimed(aliveLifetime, character, weapon));
        }

        private NetworkObject RandomCharacterPrefab() =>
            _data.CharacterPrefabs[Random.Range(0, _data.CharacterPrefabs.Length)];

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
