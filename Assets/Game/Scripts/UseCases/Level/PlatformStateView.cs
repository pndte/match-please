using Bw.Entities.Physics;
using Bw.Entities.Simulation;
using Bw.UseCases.Level.Extensions;
using UnityEngine;

namespace Bw.UseCases.Level
{
    public sealed class PlatformStateView : IStateView<PlatformState>
    {
        private readonly Transform _carriage;
        private readonly PlatformRail _rail;

        public PlatformStateView(BodyBox box, PlatformRail rail)
        {
            _carriage = box.transform;
            _rail = rail;
        }

        public void Show(PlatformState from, PlatformState to, float progress)
        {
            var position = _rail.PositionAt(from, to, progress);
            _carriage.position = new Vector3(position.x, position.y, _carriage.position.z);
        }
    }
}
