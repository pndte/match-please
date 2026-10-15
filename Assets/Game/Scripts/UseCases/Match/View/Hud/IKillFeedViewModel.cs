using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Match.View.Hud
{
    public interface IKillFeedViewModel
    {
        public ISource<KillLine> Killed { get; }
    }
}
