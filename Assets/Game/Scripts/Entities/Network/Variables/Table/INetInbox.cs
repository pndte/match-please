namespace Bw.Entities.Network.Variables
{
    public interface INetInbox
    {
        internal void Receive(ulong senderClientId, NetworkMessageHeader header, INetSyncVisitor reader);
    }
}
