using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Match
{
    public interface IReadonlyMatchBoard
    {
        public IReadonlyProperty<MatchStandings> Current { get; }
    }

    public interface IMatchBoard : IReadonlyMatchBoard
    {
        public void Apply(MatchStandings standings);
    }

    public sealed class MatchBoard : IMatchBoard
    {
        private readonly ViewableProperty<MatchStandings> _current = new(MatchStandings.None);

        public IReadonlyProperty<MatchStandings> Current => _current;

        public void Apply(MatchStandings standings) => _current.Value = standings;
    }
}
