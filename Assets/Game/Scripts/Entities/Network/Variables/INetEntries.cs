namespace Bw.Entities.Network.Variables
{
    public interface INetEntries
    {
        public INetSignal<T> Get<T>(NetSignalDeclaration<T> declaration);
        public INetProperty<T> Get<T>(NetPropertyDeclaration<T> declaration);
        internal bool TryGetEntry(ushort varId, out INetSyncEntry entry);
    }
}
