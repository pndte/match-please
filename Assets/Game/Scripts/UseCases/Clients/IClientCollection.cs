using Bw.Entities;
using Bw.Entities.Infrastructure;

namespace Bw.UseCases.Clients
{
    public interface IClientCollection
    {
        IViewableBiMap<ulong, IClient> ByIds { get; }
    }
}