namespace Bw.Entities.Network.Variables
{
    public interface INetEntries
    {
        public INetRequestSender<T> Sender<T>(NetRequestDeclaration<T> declaration);
        public INetRequestReceiver<T> Receiver<T>(NetRequestDeclaration<T> declaration);
        public INetResultSender<T> Sender<T>(NetResultDeclaration<T> declaration);
        public INetResultReceiver<T> Receiver<T>(NetResultDeclaration<T> declaration);
        public INetProperty<T> Get<T>(NetPropertyDeclaration<T> declaration);
        internal INetSyncEntry EntryWritableBy(ulong senderClientId, ushort varId);
        internal void CheckSchemaOf(ulong senderClientId, uint schemaHash); //TODO: нужен только серверу, а контракт общий для обоих пиров — сделать хеш схемы обычной записью
    }
}
