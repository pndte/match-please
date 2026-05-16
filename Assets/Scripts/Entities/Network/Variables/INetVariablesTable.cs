using Bw.Entities.Infrastructure;
using JetBrains.Collections.Viewable;

namespace Bw.Entities.Network.Variables
{
    public interface INetVariablesTable
    {
        IViewableBiMap<ushort, INetSyncEntry> PropertiesByIndex { get; }
    }
}
