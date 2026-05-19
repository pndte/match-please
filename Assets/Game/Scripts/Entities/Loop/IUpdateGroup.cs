using System.Collections.Generic;

namespace Bw.Entities.Loop
{
    public interface IUpdateGroup
    {
        IReadOnlyList<IUpdatable> Subscribers { get; }
    }
}