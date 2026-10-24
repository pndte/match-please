using System;
using System.Collections.Generic;
using Bw.Entities.Network.Ticks;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network.LagCompensation
{
    public sealed class LagCompensator : ILagCompensator, IRewindables
    {
        private readonly List<IRewindable> _rewindables = new();
        private readonly INetworkTicks _ticks;
        private readonly LagCompensationConfig _config;

        public LagCompensator(INetworkTicks ticks, LagCompensationConfig config)
        {
            _ticks = ticks;
            _config = config;
        }

        public void Track(Lifetime lifetime, IRewindable rewindable)
        {
            if (_rewindables.Contains(rewindable))
                throw new InvalidOperationException("An object rewinds once: it is already tracked.");

            _rewindables.Add(rewindable);
            lifetime.OnTermination(() => _rewindables.Remove(rewindable));
        }

        public void Rewind(Lifetime lifetime, ShooterView view)
        {
            if (!double.IsFinite(view.Interpolated) || !double.IsFinite(view.Predicted))
                throw new ArgumentOutOfRangeException(nameof(view), "Lag compensation rewinds only to finite ticks.");

            var clamped = view.Clamped(_ticks.Current - _config.RewindTicks(_ticks), _ticks.Current); //TODO: моменты, которые видел стрелок, сообщает сам клиент — читер может выбирать удобные в пределах глубины отката (backtrack); сверять с его измеренной задержкой
            for (var index = 0; index < _rewindables.Count; index++)
                _rewindables[index].Rewind(lifetime, clamped);
        }
    }
}
