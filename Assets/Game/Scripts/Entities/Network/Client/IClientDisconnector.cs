namespace Bw.Entities.Network
{
    public interface IClientDisconnector
    {
        public void Disconnect(ulong clientId, string reason);
    }
}
