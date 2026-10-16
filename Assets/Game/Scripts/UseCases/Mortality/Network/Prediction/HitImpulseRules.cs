using Bw.Entities.Network.Prediction.Events;
using Bw.UseCases.Movement;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.UseCases.Mortality.Network.Prediction
{
    public sealed class HitImpulseRules : IEffectRules<RigidState, Hit>
    {
        private readonly PhysicsBody _body;
        private readonly DynamicBodyConfig _config;

        public HitImpulseRules(PhysicsBody body, DynamicBodyConfig config)
        {
            _body = body;
            _config = config;
        }

        public RigidState Apply(RigidState state, Hit hit)
        {
            var rotation = Quaternion.Euler(0f, 0f, state.Angle);
            var center = state.Position + (Vector2)(rotation * _body.localCenterOfMass);
            var arm = state.Position + (Vector2)(rotation * hit.LocalPoint) - center;
            var impulse = hit.Knockback * _config.KnockbackMass;
            var spin = (arm.x * impulse.y - arm.y * impulse.x) / _body.rotationalInertia * _config.Spin * Mathf.Rad2Deg;
            return new RigidState(state.Position, state.Angle, state.Velocity + impulse / _body.mass, state.AngularVelocity + spin);
        }
    }
}
