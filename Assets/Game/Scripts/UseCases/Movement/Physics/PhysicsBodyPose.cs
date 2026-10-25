using Bw.UseCases.Movement.Physics.Abstractions;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.UseCases.Movement.Physics
{
    public sealed class PhysicsBodyPose : IBodyPose
    {
        private readonly PhysicsBody _body;

        public PhysicsBodyPose(PhysicsBody body)
        {
            _body = body;
        }

        public void Place(Vector2 position, float angle)
        {
            var pose = new PhysicsTransform(position, new PhysicsRotate(angle * Mathf.Deg2Rad));
            var current = _body.transform;
            if (current.position != pose.position || current.rotation.angle != pose.rotation.angle)
                _body.transform = pose;
        }
    }
}
