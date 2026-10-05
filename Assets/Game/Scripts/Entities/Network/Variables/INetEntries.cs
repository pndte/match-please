namespace Bw.Entities.Network.Variables
{
    public interface INetEntries
    {
        public INetSignal<T> Get<T>(NetSignalDeclaration<T> declaration);
        public INetProperty<T> Get<T>(NetPropertyDeclaration<T> declaration);
        internal INetSyncEntry EntryWritableBy(ulong senderClientId, ushort varId);
        internal void CheckSchemaOf(ulong senderClientId, uint schemaHash); //TODO: нужен только серверу, а контракт общий для обоих пиров — сделать хеш схемы обычной записью
    }
}
