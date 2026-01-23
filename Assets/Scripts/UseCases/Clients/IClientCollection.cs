using System.Collections.Generic;
using Bw.Entities;
using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Clients
{
    public interface IClientCollection
    {
        IViewableMap<ulong, IClient> ByIds { get; }
        ICollection<IClient> All { get; }
    }
}