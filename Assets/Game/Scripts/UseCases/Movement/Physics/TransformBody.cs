using Bw.UseCases.Movement.Physics.Abstractions;
using UnityEngine;

namespace Bw.UseCases.Movement.Physics
{
    public sealed class TransformBody : IMovementBody
    {
        public Vector2 Position => _transform.position;

        private readonly Rigidbody2D _rigidbody;
        private readonly Transform _transform;

        public TransformBody(Rigidbody2D rigidbody)
        {
            _rigidbody = rigidbody;
            _transform = rigidbody.transform;
        }

        public void Place(Vector2 position)
        {
            _rigidbody.position = position;
            _transform.position = new Vector3(position.x, position.y, _transform.position.z);
        }
    }
}
