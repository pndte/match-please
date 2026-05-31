using Bw.Entities.Infrastructure;

namespace Bw.Entities.Network.Variables
{
    public interface INetVariablesTable
    {
        IViewableBiMap<ushort, INetSyncEntry> PropertiesByIndex { get; }
    }
}
