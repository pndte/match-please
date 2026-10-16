using Bw.Entities.Simulation;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.UseCases.Movement
{
    public sealed class RigidPoseView : IStateView<RigidState>
    {
        private readonly Rigidbody2D _rigidbody;
        private readonly Transform _transform;
        private readonly PhysicsBody _body;

        public RigidPoseView(Rigidbody2D rigidbody, PhysicsBody body)
        {
            _rigidbody = rigidbody;
            _transform = rigidbody.transform;
            _body = body;
        }

        public void Show(RigidState from, RigidState to, float progress)
        {
            var position = Vector2.Lerp(from.Position, to.Position, progress);
            var angle = Mathf.LerpAngle(from.Angle, to.Angle, progress);
            _rigidbody.position = position;
            _rigidbody.rotation = angle;
            _transform.SetPositionAndRotation(new Vector3(position.x, position.y, _transform.position.z), Quaternion.Euler(0f, 0f, angle));
            _body.transform = new PhysicsTransform(position, new PhysicsRotate(angle * Mathf.Deg2Rad));
        }
    }
}
