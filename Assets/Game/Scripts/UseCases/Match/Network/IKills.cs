using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Match.Network
{
    public interface IKills
    {
        public ISource<Kill> Scored { get; }
    }
}
