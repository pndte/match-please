using System;

namespace Bw.Entities.Pool
{
    // union-like thing
    public abstract class PoolCap
    {
        public static readonly PoolCap None = new Uncapped();

        private PoolCap()
        {
        }

        public static PoolCap ReclaimOldest(int max) =>
            new ReclaimingCap(RequirePositive(max));

        public static PoolCap Throwing(int max) =>
            new ThrowingCap(RequirePositive(max));

        public abstract void Switch<TState>(TState state, Action<TState> none, Action<TState, int> reclaimOldest, Action<TState, int> throwing);

        private static int RequirePositive(int max) =>
            max > 0 ? max : throw new ArgumentOutOfRangeException(nameof(max), max, "A cap must let out at least one resource.");

        private sealed class Uncapped : PoolCap
        {
            public override void Switch<TState>(TState state, Action<TState> none, Action<TState, int> reclaimOldest, Action<TState, int> throwing) =>
                none(state);
        }

        private sealed class ReclaimingCap : PoolCap
        {
            private readonly int _max;

            public ReclaimingCap(int max)
            {
                _max = max;
            }

            public override void Switch<TState>(TState state, Action<TState> none, Action<TState, int> reclaimOldest, Action<TState, int> throwing) =>
                reclaimOldest(state, _max);
        }

        private sealed class ThrowingCap : PoolCap
        {
            private readonly int _max;

            public ThrowingCap(int max)
            {
                _max = max;
            }

            public override void Switch<TState>(TState state, Action<TState> none, Action<TState, int> reclaimOldest, Action<TState, int> throwing) =>
                throwing(state, _max);
        }
    }
}
