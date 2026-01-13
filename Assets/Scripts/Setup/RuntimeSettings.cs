using UnityEngine;

namespace Setup
{
    /// <summary>
    /// Stores the current network peer type for the game instance.
    /// This class is bound as a singleton in the ProjectContext.
    /// </summary>
    public class RuntimeSettings : IRuntimeSettings
    {
        private PeerType _currentPeerType;

        /// <inheritdoc />
        public PeerType CurrentPeerType => _currentPeerType;

        /// <summary>
        /// Initializes the runtime settings with the specified peer type.
        /// This should only be called once when the network starts.
        /// </summary>
        /// <param name="peerType">The peer type to set (Server or Client).</param>
        public void Initialize(PeerType peerType)
        {
            _currentPeerType = peerType;
            
            Debug.Log($"<color=cyan>[RuntimeSettings] Initialized with PeerType: {peerType}</color>");
        }

        /// <summary>
        /// Resets the runtime settings (useful for testing or when shutting down the network).
        /// </summary>
        public void Reset()
        {
            _currentPeerType = PeerType.Client;
            Debug.Log("[RuntimeSettings] Reset to uninitialized state.");
        }
    }
}

