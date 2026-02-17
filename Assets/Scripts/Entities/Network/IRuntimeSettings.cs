namespace Bw.Entities.Network
{
    /// <summary>
    /// Defines the network peer type for the current game instance.
    /// </summary>
    public enum PeerType
    {
        Server,
        Client
    }

    /// <summary>
    /// Provides read-only access to runtime settings for the current game instance.
    /// </summary>
    public interface IRuntimeSettings
    {
        /// <summary>
        /// Gets the current network peer type (Server or Client).
        /// This value is set when the network starts and remains immutable during runtime.
        /// </summary>
        PeerType CurrentPeerType { get; }

        public void Initialize(PeerType peerType);
    }
}

