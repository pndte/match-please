using Bw.Entities.Infrastructure;

namespace Bw.Entities.Network
{
    public interface IClientCollection
    {
        IViewableBiMap<ulong, IClient> ByIds { get; }
    }
}