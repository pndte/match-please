using Bw.Entities.Simulation;
using Bw.UseCases.Combat.Abstractions.Characters;
using Bw.UseCases.Movement;
using Bw.UseCases.Props.Extensions;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.UseCases.Props.Network
{
    public sealed class PropLauncher
    {
        private readonly IRestartable<RigidState> _body;
        private readonly PhysicsBody _physics;
        private readonly ReleaseSpots _spots;
        private readonly PropThrowConfig _config;

        public PropLauncher(
            IRestartable<RigidState> body,
            PhysicsBody physics,
            ReleaseSpots spots,
            PropThrowConfig config)
        {
            _body = body;
            _physics = physics;
            _spots = spots;
            _config = config;
        }

        public Vector2 ThrowVelocity(IReadonlyCharacter holder, float aim) =>
            _config.ThrowVelocity(aim, holder.Movement.State.Value.Body.Velocity);

        public RigidState Launched(IReadonlyCharacter holder, Vector2 velocity)
        {
            var spot = _spots.Nearest(holder, holder.Hands.Grip);
            return new RigidState(spot.Position, spot.Angle, velocity, _config.SpinOf(velocity));
        }

        public void Launch(IReadonlyCharacter holder, Vector2 velocity)
        {
            _body.Restart(Launched(holder, velocity));
            _physics.SetHitEvents(true);
        }
    }
}
