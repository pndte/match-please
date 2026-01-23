using Bw.Entities;
using Bw.Entities.Extensions;
using Bw.UseCases.Character;
using Bw.UseCases.Character.Network;
using Bw.UseCases.Clients;
using Cysharp.Threading.Tasks;
using JetBrains.Lifetimes;
using Unity.Netcode;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace Bw.UseCases.Spawning.Network
{
    /// <summary>
    /// Handles spawning of player characters when clients connect.
    /// Each client gets their own character with proper NetworkObject ownership.
    /// </summary>
    public class NetworkCharactersSpawner : NetworkBehaviour, ICharacterSpawner
    {
        [Tooltip("Spawn points for players. If empty, spawns at origin.")]
        [SerializeField]
        private Transform[] _spawnPoints;

        [Tooltip("Random offset range for spawn positions")]
        [SerializeField]
        private float _spawnRandomOffset = 2f;
        private int _nextSpawnPointIndex = 0;
        
        private ICharacterRegistry _characterRegistry;
        private INetworkPrefabInstanceHandler _prefabHandler;
        private IInstantiator _instantiator;
        [SerializeField] private GameObject _character;

        [Inject]
        private void Construct(Lifetime lifetime, IClientCollection clientCollection, ICharacterRegistry characterRegistry, INetworkPrefabInstanceHandler prefabInstanceHandler,
            IInstantiator instantiator)
        {
            _characterRegistry = characterRegistry;
            _prefabHandler = prefabInstanceHandler;
            _instantiator = instantiator;
            Debug.Log("Construct executed");
            
            clientCollection.ByIds.AdviseAdd(lifetime, async (_, client) =>
            {
                await UniTask.Yield();
                SpawnCharacterFor(lifetime, client);
            }); // TODO: спавн на лайфтайм, надо перенести в другой объект
        }

        public ICharacter SpawnCharacterFor(Lifetime lifetime, IClient client)
        {
            Debug.Log("Trying to spawn character");

            var spawnPosition = GetSpawnPosition();
            var spawnRotation = Quaternion.identity;
            var playerInstance =  _instantiator.InstantiatePrefab(_character, spawnPosition, spawnRotation, null).GetComponent<NetworkObject>();
            
            playerInstance.SpawnAsPlayerObject(client.Id, true);
            var character = playerInstance.gameObject.GetComponent<NetworkCharacter>();
            
            character.SpawnedLifetime.WhenAliveOnce(lifetime, spawnedLifetime =>
                _characterRegistry.ClientByCharacter.AddLifetimed(spawnedLifetime, character, client));

            Log($"<color=green>✓ Player character spawned for client {client} at {spawnPosition}</color>");

            return character;
        }

        private Vector3 GetSpawnPosition()
        {
            Vector3 basePosition;

            if (_spawnPoints != null && _spawnPoints.Length > 0)
            {
                var spawnPoint = _spawnPoints[_nextSpawnPointIndex];
                _nextSpawnPointIndex = (_nextSpawnPointIndex + 1) % _spawnPoints.Length;
                basePosition = spawnPoint.position;
            }
            else
            {
                basePosition = Vector3.zero;
            }

            // Add random offset to avoid players spawning on top of each other
            if (_spawnRandomOffset > 0f)
            {
                Vector3 randomOffset = new Vector3(
                    Random.Range(-_spawnRandomOffset, _spawnRandomOffset),
                    0f,
                    Random.Range(-_spawnRandomOffset, _spawnRandomOffset)
                );
                basePosition += randomOffset;
            }

            return basePosition;
        }

        private void Log(string message)
        {
            Debug.Log($"[PlayerSpawner] {message}");
        }

        #region Editor Helpers

        private void OnDrawGizmos()
        {
            if (_spawnPoints.Length == 0)
            {
                return;
            }

            Gizmos.color = Color.green;
            foreach (var spawnPoint in _spawnPoints)
            {
                Gizmos.DrawWireSphere(spawnPoint.position, 0.5f);
                Gizmos.DrawLine(spawnPoint.position, spawnPoint.position + Vector3.up * 2f);
            }
        }

        #endregion
    }
}