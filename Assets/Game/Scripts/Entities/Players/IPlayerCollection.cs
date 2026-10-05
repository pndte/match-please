using JetBrains.Collections.Viewable;

namespace Bw.Entities.Players
{
    public interface IPlayerCollection : IViewableSet<IPlayer>
    {
    }

    public sealed class PlayerCollection : ViewableSet<IPlayer>, IPlayerCollection
    {
    }
}