using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Match.View.Hud
{
    public interface IMatchViewModel
    {
        public IReadonlyProperty<bool> Shown { get; }
        public IReadonlyProperty<ScoreSheet> Sheet { get; }
        public int SecondsLeft { get; }
    }
}
