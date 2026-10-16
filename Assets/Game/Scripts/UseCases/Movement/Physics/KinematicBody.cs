using Bw.UseCases.Movement.Physics.Abstractions;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.UseCases.Movement.Physics
{
    public sealed class KinematicBody : IMovementBody
    {
        public Vector2 Position => _transform.position;

        private readonly Rigidbody2D _rigidbody;
        private readonly Transform _transform;
        private readonly PhysicsBody _body;

        public KinematicBody(Rigidbody2D rigidbody, PhysicsBody body)
        {
            _rigidbody = rigidbody;
            _transform = rigidbody.transform;
            _body = body;
        }

        public void Place(Vector2 position)
        {
            _rigidbody.position = position;
            _transform.position = new Vector3(position.x, position.y, _transform.position.z);
            _body.position = position;
        }
    }
}
