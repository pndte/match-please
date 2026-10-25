using System;

namespace Bw.UseCases.Level
{
    public readonly struct PlatformState : IEquatable<PlatformState>
    {
        public readonly float Along;

        public PlatformState(float along)
        {
            Along = along;
        }

        public bool Equals(PlatformState other) =>
            Along.Equals(other.Along);
    }
}
