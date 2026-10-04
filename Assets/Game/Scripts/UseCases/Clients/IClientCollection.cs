using Bw.Entities.Infrastructure;
using Bw.Entities.Network;

namespace Bw.UseCases.Clients
{
    public interface IClientCollection
    {
        IViewableBiMap<ulong, IClient> ByIds { get; }
    }
}