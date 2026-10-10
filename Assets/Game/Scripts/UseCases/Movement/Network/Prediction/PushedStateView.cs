using Bw.Entities.Simulation;
using Bw.UseCases.Movement.Extensions;

namespace Bw.UseCases.Movement.Network.Prediction
{
    public sealed class PushedStateView : IStateView<MovementState>
    {
        private readonly IStateView<MovementState> _view;
        private readonly IPredictedPush _push;

        public PushedStateView(IStateView<MovementState> view, IPredictedPush push)
        {
            _view = view;
            _push = push;
        }

        public void Show(MovementState from, MovementState to, float progress)
        {
            var offset = _push.Offset;
            _view.Show(from.Shifted(offset), to.Shifted(offset), progress);
        }
    }
}
