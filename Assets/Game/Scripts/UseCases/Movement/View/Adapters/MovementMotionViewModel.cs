using Bw.Entities.Simulation;

namespace Bw.UseCases.Movement.View.Adapters
{
    public sealed class MovementMotionViewModel : IStateView<MovementState>
    {
        private readonly IStateView<MovementMotion> _motion;
        private readonly MovementConfig _config;

        public MovementMotionViewModel(IStateView<MovementMotion> motion, MovementConfig config)
        {
            _motion = motion;
            _config = config;
        }

        public void Show(MovementState from, MovementState to, float progress) =>
            _motion.Show(MotionOf(from), MotionOf(to), progress);

        private MovementMotion MotionOf(MovementState state) =>
            new(state.Body.Position, state.Body.Velocity.x / _config.Speed, state.Body.Velocity.y / _config.JumpForce, state.Body.Grounded);
    }
}
