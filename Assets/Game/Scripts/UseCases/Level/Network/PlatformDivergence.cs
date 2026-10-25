using Bw.Entities.Network.Rollback;
using UnityEngine;

namespace Bw.UseCases.Level.Network
{
    public sealed class PlatformDivergence : IStateDivergence<PlatformState>
    {
        private const float Distance = 0.01f;

        private readonly PlatformRail _rail;

        public PlatformDivergence(PlatformRail rail)
        {
            _rail = rail;
        }

        public bool Diverged(PlatformState predicted, PlatformState actual) =>
            Mathf.Abs(predicted.Along - actual.Along) * _rail.Travel.magnitude > Distance;
    }
}
