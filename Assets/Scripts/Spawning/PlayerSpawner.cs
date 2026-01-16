using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace Spawning
{
    /// <summary>
    /// Handles spawning of player characters when clients connect.
    /// Each client gets their own character with proper NetworkObject ownership.
    /// </summary>
    public class PlayerSpawner : NetworkBehaviour
    {
        [Inject] private IInstantiator _instantiator;
        
        [Header("Spawn Configuration")]
        [Tooltip("The player character prefab to spawn (must have NetworkObject component)")]
        [SerializeField] private GameObject[] playerPrefabs;
        
        [Tooltip("Spawn points for players. If empty, spawns at origin.")]
        [SerializeField] private Transform[] spawnPoints;
        
        [Tooltip("Random offset range for spawn positions")]
        [SerializeField] private float spawnRandomOffset = 2f;
        
        [Header("Debug")]
        [SerializeField] private bool enableLogging = true;

        private int _nextSpawnPointIndex = 0;

        public override void OnNetworkSpawn()
        {
            if (!IsServer)
            {
                return; // Only server handles spawning
            }

            // Subscribe to client connection events
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            
            Log("<color=green>PlayerSpawner initialized on server</color>");
        }

        public override void OnNetworkDespawn()
        {
            if (!IsServer)
            {
                return;
            }

            // Unsubscribe from events
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            }
        }

        /// <summary>
        /// Called when a client connects to the server.
        /// Spawns a player character for the connected client.
        /// </summary>
        private void OnClientConnected(ulong clientId)
        {
            Log($"<color=cyan>Client {clientId} connected. Spawning player character...</color>");

            if (playerPrefabs == null)
            {
                Debug.LogError("[PlayerSpawner] Player prefab is not assigned! Cannot spawn player.");
                return;
            }

            // Get spawn position
            Vector3 spawnPosition = GetSpawnPosition();
            Quaternion spawnRotation = Quaternion.identity;

            // Instantiate the player character
            GameObject playerInstance = _instantiator.InstantiatePrefab(playerPrefabs[Random.Range(0, playerPrefabs.Length)], spawnPosition, spawnRotation, null);
            
            // Get the NetworkObject component
            NetworkObject networkObject = playerInstance.GetComponent<NetworkObject>();
            if (networkObject == null)
            {
                Debug.LogError("[PlayerSpawner] Player prefab does not have a NetworkObject component!");
                Destroy(playerInstance);
                return;
            }

            // Spawn the NetworkObject and assign ownership to the client
            networkObject.SpawnAsPlayerObject(clientId, true);
            
            Log($"<color=green>✓ Player character spawned for client {clientId} at {spawnPosition}</color>");
        }

        /// <summary>
        /// Called when a client disconnects from the server.
        /// The player character will be automatically despawned by NetworkManager.
        /// </summary>
        private void OnClientDisconnected(ulong clientId)
        {
            Log($"<color=yellow>Client {clientId} disconnected. Player character will be despawned automatically.</color>");
        }

        /// <summary>
        /// Gets the next spawn position based on spawn points or random offset.
        /// </summary>
        private Vector3 GetSpawnPosition()
        {
            Vector3 basePosition;

            if (spawnPoints != null && spawnPoints.Length > 0)
            {
                // Use spawn points in round-robin fashion
                Transform spawnPoint = spawnPoints[_nextSpawnPointIndex];
                _nextSpawnPointIndex = (_nextSpawnPointIndex + 1) % spawnPoints.Length;
                basePosition = spawnPoint.position;
            }
            else
            {
                // Default spawn at origin
                basePosition = Vector3.zero;
            }

            // Add random offset to avoid players spawning on top of each other
            if (spawnRandomOffset > 0f)
            {
                Vector3 randomOffset = new Vector3(
                    Random.Range(-spawnRandomOffset, spawnRandomOffset),
                    0f,
                    Random.Range(-spawnRandomOffset, spawnRandomOffset)
                );
                basePosition += randomOffset;
            }

            return basePosition;
        }

        private void Log(string message)
        {
            if (enableLogging)
            {
                Debug.Log($"[PlayerSpawner] {message}");
            }
        }

        #region Editor Helpers
        
        private void OnDrawGizmos()
        {
            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                return;
            }

            // Draw spawn points in the editor
            Gizmos.color = Color.green;
            foreach (Transform spawnPoint in spawnPoints)
            {
                if (spawnPoint != null)
                {
                    Gizmos.DrawWireSphere(spawnPoint.position, 0.5f);
                    Gizmos.DrawLine(spawnPoint.position, spawnPoint.position + Vector3.up * 2f);
                }
            }
        }
        
        #endregion
    }
}

