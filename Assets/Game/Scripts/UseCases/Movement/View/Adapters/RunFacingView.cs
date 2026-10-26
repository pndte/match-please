using Bw.Entities.Simulation;

namespace Bw.UseCases.Movement.View.Adapters
{
    public sealed class RunFacingView : IStateView<MovementState>
    {
        private readonly IStateView<MovementState> _view;
        private readonly RunFacingViewModel _facing;

        public RunFacingView(IStateView<MovementState> view, RunFacingViewModel facing)
        {
            _view = view;
            _facing = facing;
        }

        public void Show(MovementState from, MovementState to, float progress)
        {
            _facing.Follow(from, to, progress);
            _view.Show(from, to, progress);
        }
    }
}
