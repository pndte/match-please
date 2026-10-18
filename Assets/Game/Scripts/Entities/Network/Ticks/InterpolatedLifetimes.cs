using JetBrains.Lifetimes;
using R3;

namespace Bw.Entities.Network.Ticks
{
    public sealed class InterpolatedLifetimes : IInterpolatedLifetimes
    {
        private readonly IServerTime _time;
        private readonly IInterpolationTicks _ticks;

        public InterpolatedLifetimes(IServerTime time, IInterpolationTicks ticks)
        {
            _time = time;
            _ticks = ticks;
        }

        public Lifetime CreateUntilShown(Lifetime owner)
        {
            var now = _time.Tick;
            var shown = new LifetimeDefinition(owner);
            Observable.EveryUpdate(UnityFrameProvider.Update, shown.Lifetime)
                .Where(_ => _ticks.InterpolationTick >= now)
                .Take(1)
                .Subscribe(_ => shown.Terminate());
            return shown.Lifetime;
        }
    }
}
