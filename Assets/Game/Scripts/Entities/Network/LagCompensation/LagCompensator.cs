using System;
using Bw.Entities.Network.Ticks;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network.LagCompensation
{
    public sealed class LagCompensator : ILagCompensator
    {
        private readonly IRewindableCollection _rewindables;
        private readonly IScheduledCollection _scheduled;
        private readonly INetworkTicks _ticks;
        private readonly LagCompensationConfig _config;
        private readonly Action _restoreScheduled;

        public LagCompensator(IRewindableCollection rewindables, IScheduledCollection scheduled, INetworkTicks ticks, LagCompensationConfig config)
        {
            _rewindables = rewindables;
            _scheduled = scheduled;
            _ticks = ticks;
            _config = config;
            _restoreScheduled = RestoreScheduled;
        }

        public void Rewind(Lifetime lifetime, SeenTicks seen)
        {
            if (!double.IsFinite(seen.Interpolated) || !double.IsFinite(seen.Predicted))
                throw new ArgumentOutOfRangeException(nameof(seen), "Lag compensation rewinds only to finite ticks.");

            var clamped = new SeenTicks(Clamped(seen.Interpolated), Clamped(seen.Predicted)); //TODO: моменты, которые видел стрелок, сообщает сам клиент — читер может выбирать удобные в пределах глубины отката (backtrack); сверять с его измеренной задержкой
            _scheduled.PoseAt(clamped.Predicted);
            lifetime.OnTermination(_restoreScheduled);

            for (var index = 0; index < _rewindables.Count; index++)
                _rewindables[index].Rewind(lifetime, clamped);
        }

        private double Clamped(double tick) =>
            Math.Clamp(tick, _ticks.Current - _config.RewindTicks(_ticks), _ticks.Current);

        private void RestoreScheduled() =>
            _scheduled.PoseAt(_ticks.Current);
    }
}
