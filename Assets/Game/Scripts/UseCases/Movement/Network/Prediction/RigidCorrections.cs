using Bw.Entities.Network.Prediction.Stream;
using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Movement.Network.Prediction
{
    public sealed class RigidCorrections : IStateCorrections<RigidState>
    {
        public ISource<StateCorrection<RigidState>> Corrected => _corrected;
        public bool IsPending => _pending;

        private readonly Signal<StateCorrection<RigidState>> _corrected = new();
        private readonly RigidStateHistory _history;

        private bool _pending;
        private int _tick;
        private double _shownTick;
        private RigidState _shown;

        public RigidCorrections(RigidStateHistory history)
        {
            _history = history;
        }

        public void Remember(int tick, double shownTick, RigidState shown)
        {
            _pending = true;
            _tick = tick;
            _shownTick = shownTick;
            _shown = shown;
        }

        public void Forget() =>
            _pending = false;

        public void Reach(int tick)
        {
            if (!_pending || tick != _tick)
                return;

            _pending = false;
            var after = _history.Interpolated(_shownTick);
            if (!_shown.Equals(after))
                _corrected.Fire(new StateCorrection<RigidState>(_shown, after));
        }
    }
}
