using System;
using Bw.Entities;
using Bw.Entities.Extensions;
using Bw.Entities.Network;
using Bw.Entities.Players;
using Bw.UseCases.Character;
using Bw.UseCases.Character.Extensions;
using Bw.UseCases.Character.Network;
using Bw.UseCases.Shooting.Weapon;
using Cysharp.Threading.Tasks;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using Unity.Netcode;
using UnityEngine;
using Zenject;
using Bw.UseCases;
using Random = UnityEngine.Random;

namespace Bw.UseCases.Spawning.Network
{
    public class NetworkCharactersSpawner : ICharacterSpawner
    {
        public struct Data
        {
            public Transform[] SpawnPoints;
            public float SpawnRandomOffset;
            public NetworkObject CharacterPrefab;
            public NetworkObject WeaponPrefab;
        }

        private readonly Data _data;
        private readonly ICharacterRegistry _characterRegistry;
        private readonly IGameObjectByCharacterCollection _gameObjectByCharacterCollection;
        private readonly IClientPlayerCollection _clientPlayerCollection;
        private readonly DiContainer _container;
        private int _nextSpawnPointIndex;
        private int _weaponNameCounter;

        private NetworkCharactersSpawner(
            Lifetime lifetime,
            IPlayerCollection players,
            ICharacterRegistry characterRegistry,
            IGameObjectByCharacterCollection gameObjectByCharacterCollection,
            IClientPlayerCollection clientPlayerCollection,
            DiContainer container,
            Data data)
        {
            _characterRegistry = characterRegistry;
            _gameObjectByCharacterCollection = gameObjectByCharacterCollection;
            _clientPlayerCollection = clientPlayerCollection;
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

            var characterObject = NetworkPrefabInstantiationHelper.Instantiate(
                _container, _data.CharacterPrefab, spawnPosition, spawnRotation);

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

            SpawnAndAttachWeapon(player, client, characterHolder, characterLifetime, characterObject.transform.position, spawnRotation).Forget();

            Debug.Log($"[PlayerSpawner] Player character spawned for client {client.Id} at {spawnPosition}");
            return character;
        }

        private async UniTaskVoid SpawnAndAttachWeapon(
            IPlayer player,
            IClient client,
            CharacterHolder characterHolder,
            Lifetime characterLifetime,
            Vector3 position,
            Quaternion rotation)
        {
            var weaponObject = NetworkPrefabInstantiationHelper.Instantiate(
                _container, _data.WeaponPrefab, position, rotation);
            weaponObject.name += $", {_weaponNameCounter++}";

            if (weaponObject.TryGetComponent(out Rigidbody2D weaponRb))
                weaponRb.simulated = false;

            weaponObject.SpawnWithOwnership(client.Id, destroyWithScene: true);

            var weaponHolder = RequireComponent<WeaponHolder>(weaponObject.gameObject);

            await UniTask.Delay(TimeSpan.FromSeconds(3));//TODO: костыль, фиксить
            characterHolder.Value.State.WhenAlive(characterLifetime, aliveLifetime =>
            {
                weaponHolder.OwnershipController.AddOwner(aliveLifetime, player);
                weaponHolder.ControlledBy.Set(aliveLifetime, player);
                weaponHolder.PickUpWeapon(aliveLifetime, characterHolder);
            });
        }

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
