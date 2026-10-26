using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Combat.View.Adapters.Hit
{
    public sealed class ShotLaunches
    {
        public ISource<ShotLaunch> Launched => _launched;

        private readonly Signal<ShotLaunch> _launched = new();

        public void Launch(ShotLaunch launch) =>
            _launched.Fire(launch);
    }
}
