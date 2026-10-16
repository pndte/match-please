using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.Entities.Physics
{
    public sealed class SweptBody
    {
        private readonly PhysicsBody _body;

        private PhysicsTransform _settled;
        private PhysicsTransform _placed;

        public SweptBody(PhysicsBody body)
        {
            _body = body;
            _settled = body.transform;
            _placed = _settled;
        }

        public void Place(Vector2 position) =>
            Place(new PhysicsTransform(position));

        public void Place(PhysicsTransform pose)
        {
            _placed = pose;
            _body.transform = pose;
        }

        public void Sweep(float duration)
        {
            _body.transform = _settled;
            _body.linearVelocity = (_placed.position - _settled.position) / duration;
            _body.angularVelocity = Mathf.DeltaAngle(_settled.rotation.angle * Mathf.Rad2Deg, _placed.rotation.angle * Mathf.Rad2Deg) / duration;
        }

        public void Settle()
        {
            _body.transform = _placed;
            _body.linearVelocity = Vector2.zero;
            _body.angularVelocity = 0f;
            _settled = _placed;
        }
    }
}
