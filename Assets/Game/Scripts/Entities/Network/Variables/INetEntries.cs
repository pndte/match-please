namespace Bw.Entities.Network.Variables
{
    public interface INetEntries
    {
        public INetSignal<T> Get<T>(NetSignalDeclaration<T> declaration);
        public INetProperty<T> Get<T>(NetPropertyDeclaration<T> declaration);
        internal INetSyncEntry EntryWritableBy(ulong senderClientId, ushort varId);
    }
}
