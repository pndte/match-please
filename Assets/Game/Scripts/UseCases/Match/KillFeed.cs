using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Match
{
    public interface IReadonlyKillFeed
    {
        public ISource<KillNote> Reported { get; }
    }

    public interface IKillFeed : IReadonlyKillFeed
    {
        public void Report(KillNote kill);
    }

    public sealed class KillFeed : IKillFeed
    {
        private readonly Signal<KillNote> _reported = new();

        public ISource<KillNote> Reported => _reported;

        public void Report(KillNote kill) =>
            _reported.Fire(kill);
    }
}
