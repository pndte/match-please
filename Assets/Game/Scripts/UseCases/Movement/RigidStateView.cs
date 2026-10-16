using Bw.Entities.Physics;
using Bw.Entities.Simulation;
using UnityEngine;

namespace Bw.UseCases.Movement
{
    public sealed class RigidStateView : IStateView<RigidState>
    {
        private readonly Transform _transform;

        public RigidStateView(BodyBox box)
        {
            _transform = box.transform;
        }

        public void Show(RigidState from, RigidState to, float progress)
        {
            var position = Vector2.Lerp(from.Position, to.Position, progress);
            var angle = Mathf.LerpAngle(from.Angle, to.Angle, progress);
            _transform.SetPositionAndRotation(new Vector3(position.x, position.y, _transform.position.z), Quaternion.Euler(0f, 0f, angle));
        }
    }
}
