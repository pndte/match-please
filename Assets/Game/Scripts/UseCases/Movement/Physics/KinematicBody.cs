using Bw.Entities.Physics;
using Bw.UseCases.Movement.Physics.Abstractions;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.UseCases.Movement.Physics
{
    public sealed class KinematicBody : IMovementBody
    {
        public Vector2 Position => _transform.position;
        public float Angle => _body.rotation.angle * Mathf.Rad2Deg;

        private readonly Transform _transform;
        private readonly PhysicsBody _body;

        public KinematicBody(BodyBox box, PhysicsBody body)
        {
            _transform = box.transform;
            _body = body;
        }

        public void Place(Vector2 position) =>
            _transform.position = new Vector3(position.x, position.y, _transform.position.z);
    }
}
