using Bw.Entities.Simulation;
using UnityEngine;

namespace Bw.UseCases.Movement
{
    public sealed class RigidStateView : IStateView<RigidState>
    {
        private readonly Rigidbody2D _rigidbody;
        private readonly Transform _transform;

        public RigidStateView(Rigidbody2D rigidbody)
        {
            _rigidbody = rigidbody;
            _transform = rigidbody.transform;
        }

        public void Show(RigidState from, RigidState to, float progress)
        {
            var position = Vector2.Lerp(from.Position, to.Position, progress);
            var angle = Mathf.LerpAngle(from.Angle, to.Angle, progress);
            _rigidbody.position = position;
            _rigidbody.rotation = angle;
            _transform.SetPositionAndRotation(new Vector3(position.x, position.y, _transform.position.z), Quaternion.Euler(0f, 0f, angle));
        }
    }
}
