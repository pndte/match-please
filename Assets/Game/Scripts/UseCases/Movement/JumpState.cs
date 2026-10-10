using System;

namespace Bw.UseCases.Movement
{
    public readonly struct JumpState : IEquatable<JumpState>
    {
        public readonly bool Rising;
        public readonly int CoyoteTicks;
        public readonly int BufferedTicks;

        public JumpState(bool rising, int coyoteTicks, int bufferedTicks)
        {
            Rising = rising;
            CoyoteTicks = coyoteTicks;
            BufferedTicks = bufferedTicks;
        }

        public bool Equals(JumpState other) =>
            Rising == other.Rising
            && CoyoteTicks == other.CoyoteTicks
            && BufferedTicks == other.BufferedTicks;
    }
}
