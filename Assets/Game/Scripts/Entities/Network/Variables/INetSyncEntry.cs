using JetBrains.Collections.Viewable;

namespace Bw.Entities.Network.Variables
{
    public interface INetSyncEntry
    {
        internal IViewableProperty<bool> Dirty { get; }
        internal void Accept(INetSyncVisitor visitor);
    }
}
