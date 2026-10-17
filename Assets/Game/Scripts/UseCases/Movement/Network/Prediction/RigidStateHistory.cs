using System;
using UnityEngine;

namespace Bw.UseCases.Movement.Network.Prediction
{
    public sealed class RigidStateHistory
    {
        public const int Capacity = 64;

        public int NewestTick => _newestTick;
        public RigidState Newest => At(_newestTick);

        public RigidState Oldest
        {
            get
            {
                var oldest = int.MaxValue;
                for (var slot = 0; slot < Capacity; slot++)
                    if (_ticks[slot] != int.MinValue)
                        oldest = Math.Min(oldest, _ticks[slot]);

                return At(oldest);
            }
        }

        private readonly int[] _ticks = Unrecorded();
        private readonly RigidState[] _states = new RigidState[Capacity];

        private int _newestTick = int.MinValue;

        public void Record(int tick, RigidState state)
        {
            var slot = Slot(tick);
            _ticks[slot] = tick;
            _states[slot] = state;
            _newestTick = Math.Max(_newestTick, tick);
        }

        public void Clear()
        {
            Array.Fill(_ticks, int.MinValue);
            _newestTick = int.MinValue;
        }

        public bool TryGetAt(int tick, out RigidState state)
        {
            var slot = Slot(tick);
            state = _states[slot];
            return _ticks[slot] == tick;
        }

        public RigidState At(int tick) =>
            TryGetAt(tick, out var state) ? state : throw new InvalidOperationException($"No state is recorded at tick {tick}.");

        public RigidState Interpolated(double tick)
        {
            var from = (int)Math.Floor(tick);
            var before = At(from);
            var after = TryGetAt(from + 1, out var next) ? next : Newest;
            var progress = (float)(tick - from);
            return new RigidState(
                Vector2.Lerp(before.Position, after.Position, progress),
                Mathf.LerpAngle(before.Angle, after.Angle, progress),
                Vector2.Lerp(before.Velocity, after.Velocity, progress),
                Mathf.Lerp(before.AngularVelocity, after.AngularVelocity, progress));
        }

        private static int Slot(int tick) =>
            (tick % Capacity + Capacity) % Capacity;

        private static int[] Unrecorded()
        {
            var ticks = new int[Capacity];
            Array.Fill(ticks, int.MinValue);
            return ticks;
        }
    }
}
