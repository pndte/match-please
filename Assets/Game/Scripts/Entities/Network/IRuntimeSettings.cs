namespace Bw.Entities.Network
{
    public enum PeerType
    {
        Server,
        Client
    }

    public interface IRuntimeSettings
    {
        PeerType CurrentPeerType { get; }
    }
}
