using System;
using Bw.Entities;
using Bw.Entities.Extensions;
using Bw.Entities.Extensions.Zenject;
using Bw.UseCases.Character;
using Bw.UseCases.Character.Extensions;
using Bw.UseCases.Character.Network;
using Bw.UseCases.Clients;
using Bw.UseCases.Players;
using Bw.UseCases.Shooting.Weapon;
using Cysharp.Threading.Tasks;
using JetBrains.Lifetimes;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace Bw.UseCases.Spawning.Network
{
    /// <summary>
    /// Handles spawning of player characters when clients connect.
    /// Each client gets their own character with proper NetworkObject ownership.
    /// </summary>
    public class NetworkCharactersSpawner : ICharacterSpawner
    {
        public struct Data
        {
            public Transform[] SpawnPoints;
            public float SpawnRandomOffset;

            public NetworkObject Character;
            public NetworkObject Weapon;
        }

        private readonly Data _data;
        private int _nextSpawnPointIndex = 0;

        private readonly ICharacterRegistry _characterRegistry;
        private readonly IGameObjectByCharacterCollection _gameObjectByCharacterCollection;
        private readonly IClientPlayerCollection _clientPlayerCollection;
        private readonly IInstantiator _instantiator;
        private int _counter = 0;

        private NetworkCharactersSpawner(
            Lifetime lifetime,
            IClientCollection clientCollection,
            ICharacterRegistry characterRegistry,
            IGameObjectByCharacterCollection gameObjectByCharacterCollection,
            IClientPlayerCollection clientPlayerCollection,
            IInstantiator instantiator,
            Data data)
        {
            _characterRegistry = characterRegistry;
            _gameObjectByCharacterCollection = gameObjectByCharacterCollection;
            _clientPlayerCollection = clientPlayerCollection;
            _instantiator = instantiator;
            _data = data;

            clientCollection.ByIds.AdviseAdd(lifetime, (_, client) =>
                SpawnCharacterFor(lifetime, client));
        }

        public ICharacter SpawnCharacterFor(Lifetime lifetime, IClient client)
        {
            Debug.Log("Trying to spawn character");

            var spawnPosition = GetSpawnPosition();
            var spawnRotation = Quaternion.identity;
            
            var playerInstance = _instantiator.InstantiatePrefabForComponent<NetworkObject>(_data.Character, spawnPosition, spawnRotation, null);
            playerInstance.SpawnAsPlayerObject(client.Id, true);
            var characterHolder = playerInstance.gameObject.GetComponent<CharacterHolder>(); //TODO: добавлять в отдельном файле
            
            var characterObjectLifetime = characterHolder.gameObject.Lifetime();
            var character = characterHolder.Value;
            
            _gameObjectByCharacterCollection.AddLifetimed(characterObjectLifetime, character, playerInstance.gameObject); // TODO: берём лайфтайм из аргументов
            _characterRegistry.ClientByCharacter.AddLifetimed(characterObjectLifetime, character, client);
           
            SetupWeaponForPlayer();

            Log($"<color=green>✓ Player character spawned for client {client} at {spawnPosition}</color>");

            return characterHolder.Value;

            void SetupWeaponForPlayer()
            {
                var weaponObject = _instantiator.InstantiatePrefabForComponent<NetworkObject>(lifetime, _data.Weapon, spawnPosition, spawnRotation); //TODO: вынести
                weaponObject.name += ", " + _counter++;
                weaponObject.SpawnWithOwnership(client.Id, true);

                var weaponHolder = weaponObject.GetComponent<WeaponHolder>();
                var player = _clientPlayerCollection.ByClient[client];

                characterHolder.Value.State.WhenAlive(characterObjectLifetime, async aliveLifetime =>
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(2f));
                    weaponHolder.ControlledBy.Set(aliveLifetime, player);
                    weaponHolder.Ownership.AddOwner(aliveLifetime, player);
                    weaponHolder.PickUpWeapon(aliveLifetime, characterHolder);
                });
            }
        }


        private Vector3 GetSpawnPosition()
        {
            Vector3 basePosition;

            if (_data.SpawnPoints is { Length: > 0 })
            {
                var spawnPoint = _data.SpawnPoints[_nextSpawnPointIndex];
                _nextSpawnPointIndex = (_nextSpawnPointIndex + 1) % _data.SpawnPoints.Length;
                basePosition = spawnPoint.position;
            }
            else
            {
                basePosition = Vector3.zero;
            }

            // Add random offset to avoid players spawning on top of each other
            if (_data.SpawnRandomOffset > 0f)
            {
                Vector3 randomOffset = new Vector3(
                    Random.Range(-_data.SpawnRandomOffset, _data.SpawnRandomOffset),
                    0f,
                    Random.Range(-_data.SpawnRandomOffset, _data.SpawnRandomOffset)
                );
                basePosition += randomOffset;
            }

            return basePosition;
        }

        private void Log(string message)
        {
            Debug.Log($"[PlayerSpawner] {message}");
        }
    }
}