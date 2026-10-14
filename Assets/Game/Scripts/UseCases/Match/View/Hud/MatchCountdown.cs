using Bw.Entities.Network.Ticks;
using Bw.Entities.Simulation;
using UnityEngine;

namespace Bw.UseCases.Match.View.Hud
{
    public sealed class MatchCountdown
    {
        public int SecondsLeft =>
            Mathf.Max(0, Mathf.CeilToInt((_board.Current.Value.Period.EndTick - _serverTime.Tick) * _step.Duration));

        private readonly IReadonlyMatchBoard _board;
        private readonly IServerTime _serverTime;
        private readonly ISimulationStep _step;

        public MatchCountdown(IReadonlyMatchBoard board, IServerTime serverTime, ISimulationStep step)
        {
            _board = board;
            _serverTime = serverTime;
            _step = step;
        }
    }
}
