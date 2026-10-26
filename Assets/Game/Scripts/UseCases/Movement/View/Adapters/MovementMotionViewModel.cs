using Bw.Entities.Simulation;
using UnityEngine;

namespace Bw.UseCases.Movement.View.Adapters
{
    public sealed class MovementMotionViewModel : IStateView<MovementState>
    {
        private const float MovedSmoothing = 0.08f;
        private const float MovedCeiling = 2f;

        private readonly IStateView<MovementMotion> _motion;
        private readonly MovementConfig _config;

        private Vector2 _drawn;
        private float _moved = MovedCeiling;

        public MovementMotionViewModel(IStateView<MovementMotion> motion, MovementConfig config)
        {
            _motion = motion;
            _config = config;
        }

        public void Show(MovementState from, MovementState to, float progress)
        {
            Measure(Vector2.Lerp(from.Body.Position, to.Body.Position, progress));
            _motion.Show(MotionOf(from), MotionOf(to), progress);
        }

        private void Measure(Vector2 drawn)
        {
            var frame = Time.deltaTime;
            if (frame <= 0f)
                return;

            var moved = Mathf.Min(Mathf.Abs(drawn.x - _drawn.x) / frame / _config.Speed, MovedCeiling);
            _drawn = drawn;
            _moved = Mathf.Lerp(_moved, moved, 1f - Mathf.Exp(-frame / MovedSmoothing));
        }

        private MovementMotion MotionOf(MovementState state) =>
            new(state.Body.Position, Moved(state.Body.Velocity.x / _config.Speed), state.Body.Velocity.y / _config.JumpForce, state.Body.Grounded);

        private float Moved(float run) =>
            Mathf.Sign(run) * Mathf.Min(Mathf.Abs(run), _moved);
    }
}
