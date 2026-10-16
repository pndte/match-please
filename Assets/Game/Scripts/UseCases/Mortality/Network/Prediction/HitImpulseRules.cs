using Bw.Entities.Network.Prediction.Events;
using Bw.UseCases.Movement;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.UseCases.Mortality.Network.Prediction
{
    public sealed class HitImpulseRules : IEffectRules<RigidState, Impacts>
    {
        private readonly PhysicsBody _body;
        private readonly HitImpulseConfig _config;

        public HitImpulseRules(PhysicsBody body, HitImpulseConfig config)
        {
            _body = body;
            _config = config;
        }

        public RigidState Apply(RigidState state, Impacts impacts)
        {
            var rotation = Quaternion.Euler(0f, 0f, state.Angle);
            var center = state.Position + (Vector2)(rotation * _body.localCenterOfMass);
            var velocity = state.Velocity;
            var angularVelocity = state.AngularVelocity;
            foreach (var impact in impacts)
            {
                var arm = state.Position + (Vector2)(rotation * impact.Contact.Point) - center;
                var impulse = impact.Hit.Knockback * _config.KnockbackMass;
                velocity += impulse / _body.mass;
                angularVelocity += (arm.x * impulse.y - arm.y * impulse.x) / _body.rotationalInertia * _config.Spin * Mathf.Rad2Deg;
            }

            return new RigidState(state.Position, state.Angle, velocity, angularVelocity);
        }
    }
}
