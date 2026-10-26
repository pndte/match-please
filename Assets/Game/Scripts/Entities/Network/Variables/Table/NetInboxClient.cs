namespace Bw.Entities.Network.Variables
{
    public sealed class NetInboxClient : INetInbox
    {
        private readonly NetEntries _entries;

        public NetInboxClient(NetEntries entries)
        {
            _entries = entries;
        }

        void INetInbox.Receive(ulong senderClientId, NetworkMessageHeader header, INetSyncVisitor reader) =>
            _entries.At(header.VarId).Entry.Accept(reader);
    }
}
