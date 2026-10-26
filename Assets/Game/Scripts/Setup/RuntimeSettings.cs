using Bw.Entities.Network;

namespace Setup
{
    public sealed class RuntimeSettings : IRuntimeSettings
    {
        public PeerType CurrentPeerType { get; }

        public RuntimeSettings(PeerType currentPeerType)
        {
            CurrentPeerType = currentPeerType;
        }
    }
}
