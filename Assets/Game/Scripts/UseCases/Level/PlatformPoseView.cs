using Bw.Entities.Physics;
using Bw.Entities.Simulation;
using Bw.UseCases.Level.Extensions;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.UseCases.Level
{
    public sealed class PlatformPoseView : IStateView<PlatformState>
    {
        private readonly Transform _carriage;
        private readonly PhysicsBody _body;
        private readonly PlatformRail _rail;

        public PlatformPoseView(BodyBox box, PhysicsBody body, PlatformRail rail)
        {
            _carriage = box.transform;
            _body = body;
            _rail = rail;
        }

        public void Show(PlatformState from, PlatformState to, float progress)
        {
            var position = _rail.PositionAt(from, to, progress);
            _carriage.position = new Vector3(position.x, position.y, _carriage.position.z);
            _body.position = position;
        }
    }
}
