using JetBrains.Collections.Viewable;

namespace Bw.Entities.Network.Variables
{
    public interface INetSyncEntry
    {
        IViewableProperty<bool> Dirty { get; }
        void Accept(INetSyncVisitor visitor);
    }
}
