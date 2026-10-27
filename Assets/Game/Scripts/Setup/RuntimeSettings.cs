using Bw.Entities.Network;

namespace Setup
{
    public sealed class RuntimeSettings : IRuntimeSettings
    {
        public PeerType CurrentPeerType { get; }
        public string Level { get; }

        public RuntimeSettings(PeerType currentPeerType, string level)
        {
            CurrentPeerType = currentPeerType;
            Level = level;
        }
    }
}
