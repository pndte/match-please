using Bw.UseCases.Movement.View.Animation.Abstractions;
using UnityEngine;

namespace Bw.UseCases.Movement.View.Adapters
{
    public sealed class RunFacing : IFacingViewModel
    {
        private const float TurnSpeed = 0.05f;

        public float Facing { get; private set; } = 1f;

        private readonly MovementConfig _config;

        public RunFacing(MovementConfig config)
        {
            _config = config;
        }

        public void Follow(MovementState from, MovementState to, float progress)
        {
            var run = Mathf.Lerp(from.Body.Velocity.x, to.Body.Velocity.x, progress) / _config.Speed;
            if (Mathf.Abs(run) > TurnSpeed)
                Facing = Mathf.Sign(run);
        }
    }
}
