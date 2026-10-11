using System;

namespace Bw.UseCases.Movement.Network.Root
{
    public sealed class MovementRoot : IMovementRoot
    {
        private int _firstTick;
        private int _lastTick = -1;

        public bool RootedAt(int tick) =>
            tick >= _firstTick && tick <= _lastTick;

        public void Root(int firstTick, int lastTick)
        {
            if (lastTick < firstTick)
                throw new ArgumentOutOfRangeException(nameof(lastTick), lastTick, $"A root ends no earlier than it starts at tick {firstTick}.");

            _firstTick = firstTick > _lastTick + 1 ? firstTick : Math.Min(_firstTick, firstTick);
            _lastTick = Math.Max(_lastTick, lastTick);
        }
    }
}
