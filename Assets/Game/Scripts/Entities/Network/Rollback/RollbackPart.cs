using Bw.Entities.Network.Replication;
using Bw.Entities.Simulation;

namespace Bw.Entities.Network.Rollback
{
    public sealed class RollbackPart<TState> : IRollbackPart where TState : struct
    {
        private readonly TickHistory<TState> _saved;
        private readonly IRestorable<TState> _state;
        private readonly IReplicatedStates<TState> _server;
        private readonly IStateDivergence<TState> _divergence;

        private int _correctedTick = int.MinValue;

        public RollbackPart(
            IRestorable<TState> state,
            IReplicatedStates<TState> server,
            IStateDivergence<TState> divergence,
            WorldRollbackConfig config)
        {
            _state = state;
            _server = server;
            _divergence = divergence;
            _saved = new TickHistory<TState>(config.HistoryTicks);
        }

        public bool TryGetDivergence(out int tick)
        {
            var newest = _server.NewestTick;
            if (newest != _correctedTick && _server.TryGetAt(newest, out var server) && Diverged(newest, server))
            {
                tick = newest;
                return true;
            }

            tick = default;
            return false;
        }

        public void Save(int tick) =>
            _saved.Record(tick, _state.State.Value);

        public void Restore(int tick)
        {
            if (TryGetStaleServerState(tick, out var stale))
            {
                _correctedTick = _server.NewestTick;
                Apply(tick, stale);
                return;
            }

            Apply(tick, StateAt(tick));
        }

        private bool TryGetStaleServerState(int tick, out TState stale)
        {
            var newest = _server.NewestTick;
            if (newest < tick && newest != _correctedTick && _server.TryGetAt(newest, out stale) && Diverged(newest, stale))
                return true;

            stale = default;
            return false;
        }

        private void Apply(int tick, TState state)
        {
            _state.Apply(state);
            _saved.Record(tick, state);
        }

        private bool Diverged(int tick, TState server)
        {
            if (_saved.TryGetAt(tick, out var predicted))
                return _divergence.Diverged(predicted, server);

            return _saved.NewestTick > tick;
        }

        private TState StateAt(int tick)
        {
            if (_server.TryGetAt(tick, out var server))
                return server;

            return _saved.TryGetAt(tick, out var saved) ? saved : _state.State.Value;
        }
    }
}
