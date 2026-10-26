namespace Bw.Entities.Network.Variables
{
    internal sealed class NetEntrySlot
    {
        public ushort VarId { get; }
        public NetEntryDeclaration Declaration { get; }
        public INetSyncEntry Entry { get; }
        public INetSendGuard SendGuard { get; }

        public NetEntrySlot(
            ushort varId,
            NetEntryDeclaration declaration,
            INetSyncEntry entry,
            INetSendGuard sendGuard)
        {
            VarId = varId;
            Declaration = declaration;
            Entry = entry;
            SendGuard = sendGuard;
        }
    }
}
