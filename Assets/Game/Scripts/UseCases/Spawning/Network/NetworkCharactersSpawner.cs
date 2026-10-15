using System;
using Bw.CoreAdapters.Characters;
using Bw.Entities;
using Bw.Entities.Extensions;
using Bw.Entities.Network;
using Bw.Entities.Players;
using Bw.UseCases.Character;
using Bw.UseCases.Mortality;
using Bw.UseCases.Mortality.Extensions;
using Bw.UseCases.Mortality.Network;
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
            public CharacterCatalog Characters;
        }

        private readonly Data _data;
        private readonly ICharacterRegistry _characterRegistry;
        private readonly IGameObjectByCharacterCollection _gameObjectByCharacterCollection;
        private readonly IClientPlayerCollection _clientPlayerCollection;
        private readonly StartingLoadout _loadout;
        private readonly DiContainer _container;
        private int _nextSpawnPointIndex;

        private NetworkCharactersSpawner(
            Lifetime lifetime,
            IPlayerCollection players,
            ICharacterRegistry characterRegistry,
            IGameObjectByCharacterCollection gameObjectByCharacterCollection,
            IClientPlayerCollection clientPlayerCollection,
            StartingLoadout loadout,
            DiContainer container,
            Data data)
        {
            _characterRegistry = characterRegistry;
            _gameObjectByCharacterCollection = gameObjectByCharacterCollection;
            _clientPlayerCollection = clientPlayerCollection;
            _loadout = loadout;
            _container = container;
            _data = data;

            players.View(lifetime, (playerLifetime, player) =>
                SpawnCharacterFor(playerLifetime, player));
        }

        public IReadonlyMortal SpawnCharacterFor(Lifetime lifetime, IPlayer player)
        {
            if (!_clientPlayerCollection.ByClient.TryGetLeft(player, out var client)) //TODO: боты — спавнить персонажа и без клиента
                throw new InvalidOperationException(
                    "The player has no client: the network spawner spawns characters only for network players.");

            var spawnPosition = GetSpawnPosition();
            var spawnRotation = Quaternion.identity;

            var characterPrefab = _data.Characters.PrefabOf(client.Request.CharacterKey);
            var characterObject = NetworkPrefabInstantiationHelper.Instantiate(
                _container, characterPrefab, spawnPosition, spawnRotation);

            characterObject.SpawnAsPlayerObject(client.Id, destroyWithScene: true);

            var characterHolder = RequireComponent<ReadonlyMortalHolder>(characterObject.gameObject);
            var characterLifetime = characterHolder.gameObject.Lifetime();
            var character = characterHolder.Value;

            _gameObjectByCharacterCollection.AddLifetimed(
                characterLifetime, character, characterObject.gameObject);
            _characterRegistry.PlayerByCharacter.AddLifetimed(characterLifetime, character, player);

            var characterContext = RequireComponent<GameObjectContext>(characterObject.gameObject);
            var characterControllers = characterContext.Container.Resolve<IControllerRegistry>();
            var characterOwners = characterContext.Container.Resolve<IOwnerRegistry>();
            characterOwners.AddOwner(characterLifetime, player);
            character.State.WhenAlive(characterLifetime, aliveLifetime => characterControllers.Set(aliveLifetime, player));

            _loadout.Arm(character, characterLifetime, characterObject.transform.position, spawnRotation);

            Debug.Log($"[PlayerSpawner] Character '{characterPrefab.name}' spawned for client {client.Id} at {spawnPosition}");
            return character;
        }

        private Vector3 GetSpawnPosition()
        {
            var basePosition = _data.SpawnPoints is { Length: > 0 }
                ? _data.SpawnPoints[_nextSpawnPointIndex++ % _data.SpawnPoints.Length].position
                : Vector3.zero;

            if (_data.SpawnRandomOffset <= 0f)
                return basePosition;

            //TODO: the shift doesn't look at what stands there: spawn points are laid out with SpawnRandomOffset + 0.5 of free room, but a crate pushed next to one can take a new body inside it, and a body never pushes out of an overlap
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
