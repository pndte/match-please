using JetBrains.Lifetimes;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace DefaultNamespace
{
    /// <summary>
    /// Camera follow script that tracks the local player's character in a multiplayer environment.
    /// Uses Zenject for dependency injection and JetBrains Lifetimes for lifecycle management.
    /// Only follows the player character owned by the local client.
    /// </summary>
    public class LocalPlayerCameraFollow : MonoBehaviour // TODO: рефакторить
    {
        [Header("Configuration")]
        [Tooltip("Camera follow configuration settings")]
        [SerializeField] private CameraFollowConfig _config;
        
        [Header("Debug")]
        [Tooltip("Enable debug logging")]
        [SerializeField] private bool _enableLogging = true;
        
        [SerializeField] private Camera _camera;
        
        // State
        private Transform _targetTransform;
        private bool _isInitialized;
        private LifetimeDefinition _lifetimeDefinition;
        private Lifetime _lifetime;
        
        private void Awake()
        {
            // Initialize lifetime
            _lifetimeDefinition = new LifetimeDefinition();
            _lifetime = _lifetimeDefinition.Lifetime;
            
            // If no camera was injected, try to get the component
            if (_camera == null)
            {
                _camera = GetComponent<Camera>();
                if (_camera == null)
                {
                    _camera = Camera.main;
                }
            }
            
            if (_camera == null)
            {
                Debug.LogError("[LocalPlayerCameraFollow] No camera found! Please attach this script to a camera or ensure Camera.main exists.");
                enabled = false;
                return;
            }
            
            if (_config == null)
            {
                Debug.LogError("[LocalPlayerCameraFollow] CameraFollowConfig is not assigned!");
                enabled = false;
                return;
            }
            
            Log("LocalPlayerCameraFollow initialized, waiting for local player to spawn...");
        }
        
        private void Start()
        {
            // Start searching for the local player
            if (!_isInitialized)
            {
                TryFindLocalPlayer();
            }
        }
        
        private void LateUpdate()
        {
            // If we haven't found the local player yet, keep trying
            if (!_isInitialized)
            {
                TryFindLocalPlayer();
                return;
            }
            
            // If target is null (player despawned), reset and search again
            if (_targetTransform == null)
            {
                Log("<color=yellow>Target lost, searching for local player...</color>");
                _isInitialized = false;
                return;
            }
            
            // Follow the target smoothly
            FollowTarget();
        }
        
        /// <summary>
        /// Attempts to find the local player's NetworkObject.
        /// </summary>
        private void TryFindLocalPlayer()
        {
            // Check if NetworkManager is ready
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsClient)
            {
                return;
            }
            
            // Get the local client ID
            ulong localClientId = NetworkManager.Singleton.LocalClientId;
            
            // Search through all spawned NetworkObjects to find the local player
            foreach (var networkObject in NetworkManager.Singleton.SpawnManager.SpawnedObjectsList)
            {
                // Check if this is a player object owned by the local client
                if (networkObject.IsPlayerObject && networkObject.OwnerClientId == localClientId)
                {
                    SetTarget(networkObject.transform);
                    return;
                }
            }
        }
        
        /// <summary>
        /// Sets the target to follow and initializes the camera position.
        /// </summary>
        /// <param name="target">The transform to follow</param>
        private void SetTarget(Transform target)
        {
            _targetTransform = target;
            _isInitialized = true;
            
            // Initialize camera position immediately (no smoothing on first frame)
            if (_targetTransform != null)
            {
                Vector3 desiredPosition = _targetTransform.position + _config.Offset;
                
                if (_config.UseBoundaries)
                {
                    desiredPosition = ClampToBoundaries(desiredPosition);
                }
                
                _camera.transform.position = desiredPosition;
            }
            
            Log($"<color=green>✓ Local player found! Camera now following: {target.name}</color>");
        }
        
        /// <summary>
        /// Smoothly follows the target transform.
        /// </summary>
        private void FollowTarget()
        {
            if (_targetTransform == null) return;
            
            // Calculate desired position
            Vector3 desiredPosition = _targetTransform.position + _config.Offset;
            
            // Apply boundaries if enabled
            if (_config.UseBoundaries)
            {
                desiredPosition = ClampToBoundaries(desiredPosition);
            }
            
            // Smoothly interpolate to the desired position
            Vector3 smoothedPosition = Vector3.Lerp(
                _camera.transform.position,
                desiredPosition,
                _config.SmoothSpeed
            );
            
            _camera.transform.position = smoothedPosition;
        }
        
        /// <summary>
        /// Clamps the position to the configured boundaries.
        /// </summary>
        /// <param name="position">The position to clamp</param>
        /// <returns>The clamped position</returns>
        private Vector3 ClampToBoundaries(Vector3 position)
        {
            return new Vector3(
                Mathf.Clamp(position.x, _config.MinBounds.x, _config.MaxBounds.x),
                Mathf.Clamp(position.y, _config.MinBounds.y, _config.MaxBounds.y),
                Mathf.Clamp(position.z, _config.MinBounds.z, _config.MaxBounds.z)
            );
        }
        
        /// <summary>
        /// Manually set a target to follow (useful for testing or special cases).
        /// </summary>
        /// <param name="target">The transform to follow</param>
        public void SetTargetManually(Transform target)
        {
            if (target != null)
            {
                SetTarget(target);
            }
            else
            {
                Debug.LogWarning("[LocalPlayerCameraFollow] Attempted to set null target manually.");
            }
        }
        
        /// <summary>
        /// Clears the current target and resets initialization state.
        /// </summary>
        public void ClearTarget()
        {
            _targetTransform = null;
            _isInitialized = false;
            Log("Target cleared, will search for local player again.");
        }
        
        private void OnDestroy()
        {
            // Terminate lifetime
            _lifetimeDefinition?.Terminate();
        }
        
        private void Log(string message)
        {
            if (_enableLogging)
            {
                Debug.Log($"[LocalPlayerCameraFollow] {message}");
            }
        }
        
        #region Editor Helpers
        
        private void OnDrawGizmosSelected()
        {
            if (_config == null || !_config.UseBoundaries) return;
            
            // Draw boundary box
            Gizmos.color = Color.yellow;
            Vector3 center = (_config.MinBounds + _config.MaxBounds) / 2f;
            Vector3 size = _config.MaxBounds - _config.MinBounds;
            Gizmos.DrawWireCube(center, size);
            
            // Draw target connection
            if (_targetTransform != null && _isInitialized)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(_camera.transform.position, _targetTransform.position);
                Gizmos.DrawWireSphere(_targetTransform.position, 0.5f);
            }
        }
        
        #endregion
    }
}

