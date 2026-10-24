using System;
using Bw.Entities.Network.Rollback;
using Bw.Entities.Network.Ticks;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Match.Network
{
    public sealed class MatchReferee : IReadonlyMatch //TODO: в конце матча игра не замирает и никто не перерождается к новому — счёт просто обнуляется
    {
        public IReadonlyProperty<MatchPeriod> Period => _period;

        private readonly ViewableProperty<MatchPeriod> _period = new(MatchPeriod.Starting);
        private readonly INetworkTicks _ticks;
        private readonly MatchConfig _config;
        private readonly IScoreboard _scoreboard;

        public MatchReferee(
            Lifetime lifetime,
            INetworkTicks ticks,
            IWorldStep worldStep,
            MatchConfig config,
            IScoreboard scoreboard)
        {
            _ticks = ticks;
            _config = config;
            _scoreboard = scoreboard;

            worldStep.Stepped(TickPhase.Late).Advise(lifetime, Tick);
        }

        private void Tick(int tick)
        {
            var period = _period.Value;
            if (tick < period.EndTick)
                return;

            switch (period.Phase)
            {
                case MatchPhase.Starting:
                case MatchPhase.Ended:
                    Start(tick);
                    break;
                case MatchPhase.Running:
                    _period.Value = new MatchPeriod(MatchPhase.Ended, tick + TicksIn(_config.ResultSeconds));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Start(int tick)
        {
            _scoreboard.Reset();
            _period.Value = new MatchPeriod(MatchPhase.Running, tick + TicksIn(_config.MatchSeconds));
        }

        private int TicksIn(float seconds) =>
            Mathf.CeilToInt(seconds / _ticks.Duration);
    }
}
