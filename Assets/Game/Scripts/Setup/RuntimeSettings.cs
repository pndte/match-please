using System;
using Bw.Entities.Network;
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
        
        private bool _initialized;

        /// <summary>
        /// Initializes the runtime settings with the specified peer type.
        /// This should only be called once when the network starts.
        /// </summary>
        /// <param name="peerType">The peer type to set (Server or Client).</param>
        public void Initialize(PeerType peerType)
        {
            if (_initialized) throw new Exception("already initialized");
            
            _currentPeerType = peerType;
            _initialized = true;
            Debug.Log($"<color=cyan>[RuntimeSettings] Initialized with PeerType: {peerType}</color>");
        }
    }
}

