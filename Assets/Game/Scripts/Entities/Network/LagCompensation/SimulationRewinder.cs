using System;
using Bw.Entities.Network.Prediction.Requests;
using Bw.Entities.Network.Ticks;
using Bw.Entities.Simulation;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network.LagCompensation
{
    public sealed class SimulationRewinder<TState> : IRewindable where TState : struct
    {
        private readonly TickedState<TState>[] _states;
        private readonly IReadonlySimulation<TState> _simulation;
        private readonly IStateView<TState> _view;
        private readonly INetworkTicks _ticks;
        private readonly Action _restore;

        private int _oldest;
        private int _newest;

        public SimulationRewinder(
            Lifetime lifetime,
            IReadonlySimulation<TState> simulation,
            IStateView<TState> view,
            INetworkTicks ticks,
            LagCompensationConfig config)
        {
            _states = new TickedState<TState>[config.RewindTicks(ticks) + 1];
            _simulation = simulation;
            _view = view;
            _ticks = ticks;
            _restore = Restore;
            _oldest = ticks.Current;
            _newest = ticks.Current;
            Store(ticks.Current, simulation.State.Value);

            simulation.Stepped.Advise(lifetime, Record);
        }

        public void Rewind(Lifetime lifetime, double tick)
        {
            var from = (int)Math.Floor(tick);
            _view.Show(StateAt(from), StateAt(from + 1), (float)(tick - from));
            lifetime.OnTermination(_restore);
        }

        private void Record(TState state)
        {
            var tick = _ticks.Current;
            if (tick < _newest)
                throw new InvalidOperationException(
                    $"The simulation stepped at tick {tick.ToString()} after tick {_newest.ToString()}: its history only goes forward.");

            var last = StateAt(_newest);
            for (var gap = Math.Max(_newest + 1, tick - _states.Length + 1); gap < tick; gap++)
                Store(gap, last);

            Store(tick, state);
            _newest = tick;
            _oldest = Math.Max(_oldest, tick - _states.Length + 1);
        }

        private TState StateAt(int tick) => //TODO: тик раньше первой записи (объект появился позже момента, который видел стрелок) даёт самое старое состояние — честнее на время отката убирать объект из мира
            _states[Slot(Math.Clamp(tick, _oldest, _newest))].State;

        private void Store(int tick, TState state) =>
            _states[Slot(tick)] = new TickedState<TState>(tick, state);

        private void Restore() =>
            _view.Show(_simulation.State.Value, _simulation.State.Value, 0f);

        private int Slot(int tick) =>
            (tick % _states.Length + _states.Length) % _states.Length;
    }
}
