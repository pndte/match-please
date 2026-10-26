using System;
using Bw.Entities.Simulation;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network.LagCompensation
{
    public sealed class SimulationRewinder<TState> : IRewindable where TState : struct
    {
        private readonly SimulationHistory<TState> _history;
        private readonly IReadonlySimulation<TState> _simulation;
        private readonly IStateView<TState> _view;
        private readonly IBubbleHistory _bubble;
        private readonly Action _restore;

        public SimulationRewinder(
            SimulationHistory<TState> history,
            IReadonlySimulation<TState> simulation,
            IStateView<TState> view,
            IBubbleHistory bubble)
        {
            _history = history;
            _simulation = simulation;
            _view = view;
            _bubble = bubble;
            _restore = Restore;
        }

        public void Rewind(Lifetime lifetime, ShooterView view)
        {
            var tick = view.TickOf(_bubble);
            var from = (int)Math.Floor(tick);
            _view.Show(StateAt(from), StateAt(from + 1), (float)(tick - from));
            lifetime.OnTermination(_restore);
        }

        private TState StateAt(int tick)
        {
            if (tick > _history.NewestTick)
                return _simulation.State.Value;

            if (tick <= _history.NewestTick - _history.Capacity)
                throw new InvalidOperationException(
                    $"No state of tick {tick.ToString()} is kept: the rewind reaches deeper than the history, so the lag compensation depth is too large.");

            if (_history.TryGetAt(tick, out var state))
                return state;

            if (!_history.TryGetEarliest(tick, out _, out var first))
                throw new InvalidOperationException($"No state of tick {tick.ToString()} or later is kept, though the history holds tick {_history.NewestTick.ToString()}.");

            return first;
        }

        private void Restore() =>
            _view.Show(_simulation.State.Value, _simulation.State.Value, 0f);
    }
}
