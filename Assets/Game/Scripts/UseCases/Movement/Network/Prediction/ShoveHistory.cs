using System;
using UnityEngine;

namespace Bw.UseCases.Movement.Network.Prediction
{
    public sealed class ShoveHistory
    {
        private const int Capacity = 64;

        private readonly int[] _ticks = Unrecorded();
        private readonly Vector2[] _shoves = new Vector2[Capacity];

        public void Add(int tick, Vector2 force)
        {
            var slot = Slot(tick);
            if (_ticks[slot] != tick)
            {
                _ticks[slot] = tick;
                _shoves[slot] = Vector2.zero;
            }

            _shoves[slot] += force;
        }

        public void Forget(int tick)
        {
            var slot = Slot(tick);
            if (_ticks[slot] == tick)
                _ticks[slot] = int.MinValue;
        }

        public void Clear() =>
            Array.Fill(_ticks, int.MinValue);

        public bool TryGetAt(int tick, out Vector2 shove)
        {
            var slot = Slot(tick);
            shove = _shoves[slot];
            return _ticks[slot] == tick;
        }

        public Vector2 At(int tick) =>
            TryGetAt(tick, out var shove) ? shove : Vector2.zero;

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
