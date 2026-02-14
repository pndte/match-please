using Bw.Entities.Infrastructure;
using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Players
{
    public interface IPlayerEnabledObjects
    {
        public IViewableBiMap<IPlayer, IViewableList<ulong>> Characters { get; }
    }
}