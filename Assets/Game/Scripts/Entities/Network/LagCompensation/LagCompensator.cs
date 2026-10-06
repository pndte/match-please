using System;
using Bw.Entities.Network.Ticks;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network.LagCompensation
{
    public sealed class LagCompensator : ILagCompensator
    {
        private readonly IRewindableCollection _rewindables;
        private readonly INetworkTicks _ticks;
        private readonly LagCompensationConfig _config;

        public LagCompensator(IRewindableCollection rewindables, INetworkTicks ticks, LagCompensationConfig config)
        {
            _rewindables = rewindables;
            _ticks = ticks;
            _config = config;
        }

        public void Rewind(Lifetime lifetime, double tick)
        {
            if (!double.IsFinite(tick))
                throw new ArgumentOutOfRangeException(nameof(tick), "Lag compensation rewinds only to a finite tick.");

            var clamped = Math.Clamp(tick, _ticks.Current - _config.RewindTicks(_ticks), _ticks.Current); //TODO: момент, который видел стрелок, сообщает сам клиент — читер может выбирать удобный в пределах глубины отката (backtrack); сверять с его измеренной задержкой
            for (var index = 0; index < _rewindables.Count; index++)
                _rewindables[index].Rewind(lifetime, clamped);
        }
    }
}
