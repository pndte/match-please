using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.UseCases.Movement.Extensions
{
    public static class RigidStateBodyExtensions
    {
        public static RigidState State(this PhysicsBody body) =>
            new(body.position, body.rotation.angle * Mathf.Rad2Deg, body.linearVelocity, body.angularVelocity);

        public static void ApplyRigidState(this PhysicsBody body, RigidState state)
        {
            body.transform = new PhysicsTransform(state.Position, new PhysicsRotate(state.Angle * Mathf.Deg2Rad));
            body.linearVelocity = state.Velocity;
            body.angularVelocity = state.AngularVelocity;
        }
    }
}
